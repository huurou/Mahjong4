using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.Game.AutoPlay.Tests;

public class AutoPlayRunner_SmokeTests
{
    [Fact]
    public async Task 対局を例外なく完走する()
    {
        // Arrange
        var rules = new GameRules();
        var options = new AutoPlayOptions(GameCount: 3, Seed: 42, OutputDirectory: "./tmp-paifu-smoke", WritePaifu: false, Parallelism: 1);
        var aiFactories = new IPlayerFactory[]
        {
            new AI_v0_1_0_ランダムFactory(42),
            new AI_v0_1_0_ランダムFactory(42),
            new AI_v0_1_0_ランダムFactory(42),
            new AI_v0_1_0_ランダムFactory(42),
        };
        var playerListFactory = new MixedPlayerFactory(aiFactories, options.Seed);
        var runner = new AutoPlayRunner(
            new RoundViewProjector(),
            new ResponseCandidateEnumerator(rules),
            new TenhouResponsePriorityPolicy(),
            new DefaultResponseFactory(),
            playerListFactory,
            rules,
            options,
            NullLoggerFactory.Instance
        );

        // Act
        var report = await runner.RunAsync(3, TestContext.Current.CancellationToken);

        // Assert: 実対局数は worker 分配で指定値以上 (ceil(3/workerCount) × workerCount)
        Assert.True(report.GameCount >= 3);
        Assert.True(report.RoundCount > 0);
    }
}
