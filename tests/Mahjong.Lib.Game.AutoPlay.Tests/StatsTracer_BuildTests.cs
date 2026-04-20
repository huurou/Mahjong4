using StatsTracer = Mahjong.Lib.Game.AutoPlay.Tracing.StatsTracer;

namespace Mahjong.Lib.Game.AutoPlay.Tests;

public class StatsTracer_BuildTests
{
    [Fact]
    public void 初期状態_全カウント0を返す()
    {
        // Arrange
        var tracer = new StatsTracer();

        // Act
        var report = tracer.Build();

        // Assert
        Assert.Equal(0, report.GameCount);
        Assert.Equal(0, report.RoundCount);
        Assert.Empty(report.PlayerStats);
        Assert.Empty(report.YakuCounts);
        Assert.Empty(report.RyuukyokuCounts);
        Assert.Equal(0, report.FailedGameCount);
    }

    [Fact]
    public void RecordGameFailed_FailedGameCountが増加する()
    {
        // Arrange
        var tracer = new StatsTracer();

        // Act
        tracer.RecordGameFailed();
        tracer.RecordGameFailed();
        var report = tracer.Build();

        // Assert
        Assert.Equal(2, report.FailedGameCount);
    }
}
