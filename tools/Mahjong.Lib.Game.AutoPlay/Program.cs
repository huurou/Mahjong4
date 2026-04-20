using Mahjong.Lib.Game.AutoPlay;
using Mahjong.Lib.Game.AutoPlay.Paifu;
using Mahjong.Lib.Game.AutoPlay.Tracing;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var options = AutoPlayOptions.Parse(args);
Console.WriteLine($"[AutoPlay] Games: {options.GameCount} Seed: {options.Seed} Output: {options.OutputDirectory} WritePaifu: {options.WritePaifu}");

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var rules = new GameRules();
services.AddSingleton(rules);
services.AddSingleton<IWallGenerator>(_ => new ShuffledWallGenerator(options.Seed));
services.AddSingleton<IRoundViewProjector, RoundViewProjector>();
services.AddSingleton<IResponseCandidateEnumerator>(_ => new ResponseCandidateEnumerator(rules));
services.AddSingleton<IResponsePriorityPolicy, TenhouResponsePriorityPolicy>();
services.AddSingleton<IDefaultResponseFactory, DefaultResponseFactory>();

// AI の組み合わせはこの配列で指定する (同一 AI を 4 席並べれば単独対局)。
// MixedPlayerFactory が対局ごとに席配置をランダムシャッフルする
services.AddSingleton<IPlayerFactory>(_ =>
{
    var aiFactories = new IPlayerFactory[]
    {
        new AI_v0_1_0_ランダムFactory(options.Seed),
        new AI_v0_1_0_ランダムFactory(options.Seed),
        new AI_v0_2_0_有効牌Factory(options.Seed),
        new AI_v0_2_0_有効牌Factory(options.Seed),
    };
    return new MixedPlayerFactory(aiFactories, new Random(options.Seed));
});

services.AddSingleton<StatsTracer>();

if (options.WritePaifu)
{
    var paifuPath = Path.Combine(options.OutputDirectory, $"paifu_{options.Seed}.jsonl");
    services.AddSingleton(_ => new JsonlPaifuWriter(paifuPath));
    services.AddSingleton<PaifuRecorder>();
    services.AddSingleton<IGameTracer>(sp => new CompositeGameTracer(
        [sp.GetRequiredService<StatsTracer>(), sp.GetRequiredService<PaifuRecorder>()],
        sp.GetService<ILogger<CompositeGameTracer>>()));
}
else
{
    services.AddSingleton<IGameTracer>(sp => sp.GetRequiredService<StatsTracer>());
}

services.AddTransient<AutoPlayRunner>();

await using var provider = services.BuildServiceProvider();

var runner = provider.GetRequiredService<AutoPlayRunner>();
var statsTracer = provider.GetRequiredService<StatsTracer>();

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
    report = await runner.RunAsync(options.GameCount, statsTracer, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("キャンセルされました。");
    report = statsTracer.Build();
}

Console.WriteLine(StatsReportFormatter.Format(report));

if (options.WritePaifu)
{
    Console.WriteLine($"[AutoPlay] 牌譜を {options.OutputDirectory} に出力しました。");
}
