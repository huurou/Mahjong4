using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Scoring;
using Microsoft.Extensions.Logging.Abstractions;
using AutoPlayLib = Mahjong.Lib.Game.AutoPlay;
using StatsTracer = Mahjong.Lib.Game.AutoPlay.Tracing.StatsTracer;

namespace Mahjong.Lib.Game.AutoPlay.Tests;

public class AutoPlayRunner_SmokeTests
{
    [Fact]
    public async Task 対局を例外なく完走する()
    {
        // Arrange
        var rules = new GameRules();
        var tenpaiChecker = new TenpaiCheckerImpl();
        var statsTracer = new StatsTracer();
        var runner = new AutoPlayLib.AutoPlayRunner(
            new AutoPlayLib.ShuffledWallGenerator(42),
            new ScoreCalculatorImpl(rules),
            tenpaiChecker,
            new RoundViewProjector(),
            new ResponseCandidateEnumerator(tenpaiChecker, rules),
            new TenhouResponsePriorityPolicy(),
            new DefaultResponseFactory(),
            statsTracer,
            new AI_v0_1_0_ランダムFactory(42),
            rules,
            NullLoggerFactory.Instance
        );

        // Act
        var report = await runner.RunAsync(3, statsTracer, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, report.GameCount);
        Assert.True(report.RoundCount > 0);
    }
}
