using Mahjong.Lib.Game.AutoPlay;
using Mahjong.Lib.Game.AutoPlay.Tracing;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var options = AutoPlayOptions.Parse(args);

// stdout が端末以外 (パイプ/リダイレクト) のときはデフォルトでチャンクバッファされ、長時間ジョブの
// 10 局ごとサマリーが MB 単位の buffer が埋まるまで見えない問題が出る。AutoFlush を明示的に有効化して
// ログ/Console.WriteLine が 1 行ごとにフラッシュされるようにする。
// さらに StreamWriter に UTF-8 (no BOM) を明示し、Windows ではコンソールの出力コードページも UTF-8 に
// 揃えることで、日本語出力が CP932 として解釈されて文字化けするのを防ぐ。
var utf8NoBom = new System.Text.UTF8Encoding(false);
if (OperatingSystem.IsWindows())
{
    Console.OutputEncoding = utf8NoBom;
}
Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), utf8NoBom) { AutoFlush = true });
Console.SetError(new StreamWriter(Console.OpenStandardError(), utf8NoBom) { AutoFlush = true });

Console.WriteLine($"[AutoPlay] Games: {options.GameCount} Seed: {options.Seed} Output: {options.OutputDirectory} WritePaifu: {options.WritePaifu} LogicalProcessors: {Environment.ProcessorCount}");

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var rules = new GameRules();
services.AddSingleton(rules);
services.AddSingleton(options);
services.AddSingleton<IRoundViewProjector, RoundViewProjector>();
services.AddSingleton<IResponseCandidateEnumerator>(_ => new ResponseCandidateEnumerator(rules));
services.AddSingleton<IResponsePriorityPolicy, TenhouResponsePriorityPolicy>();
services.AddSingleton<IDefaultResponseFactory, DefaultResponseFactory>();

// AI の組み合わせはこの配列で指定する (同一 AI を 4 席並べれば単独対局)。
// MixedPlayerFactory が対局ごとに席配置を決定的にシャッフルする
services.AddSingleton(_ =>
{
    var aiFactories = new IPlayerFactory[]
    {
        new AI_v0_5_0_鳴きFactory(options.Seed),
        new AI_v0_5_0_鳴きFactory(options.Seed),
        new AI_v0_6_0_手作りFactory(options.Seed),
        new AI_v0_6_0_手作りFactory(options.Seed),
    };
    return new MixedPlayerFactory(aiFactories, options.Seed);
});

services.AddTransient<AutoPlayRunner>();

await using var provider = services.BuildServiceProvider();

var runner = provider.GetRequiredService<AutoPlayRunner>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("キャンセル要求を受信。");
    cts.Cancel();
    e.Cancel = true;
};

StatsReport report;
try
{
    report = await runner.RunAsync(options.GameCount, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("キャンセルされました。");
    return;
}

Console.WriteLine(StatsReportFormatter.Format(report));

if (options.WritePaifu)
{
    Console.WriteLine($"[AutoPlay] 牌譜を {options.OutputDirectory} に出力しました。");
}
