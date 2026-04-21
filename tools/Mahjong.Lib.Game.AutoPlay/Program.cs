using Mahjong.Lib.Game.AutoPlay;
using Mahjong.Lib.Game.AutoPlay.Tracing;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var options = AutoPlayOptions.Parse(args);
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
        new AI_v0_3_0_評価値Factory(options.Seed),
        new AI_v0_3_0_評価値Factory(options.Seed),
        new AI_v0_4_0_回し打ちFactory(options.Seed),
        new AI_v0_4_0_回し打ちFactory(options.Seed),
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
