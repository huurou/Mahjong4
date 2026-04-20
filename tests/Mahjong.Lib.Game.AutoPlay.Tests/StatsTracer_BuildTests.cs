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
        Assert.All(report.WinCounts, x => Assert.Equal(0, x));
        Assert.All(report.HoujuuCounts, x => Assert.Equal(0, x));
        Assert.Empty(report.YakuCounts);
        Assert.Empty(report.RyuukyokuCounts);
    }

    [Fact]
    public void 全プレイヤー分の統計配列が4要素で返る()
    {
        // Arrange
        var tracer = new StatsTracer();

        // Act
        var report = tracer.Build();

        // Assert
        Assert.Equal(4, report.WinCounts.Length);
        Assert.Equal(4, report.HoujuuCounts.Length);
        Assert.Equal(4, report.RiichiCounts.Length);
        Assert.Equal(4, report.CallCounts.Length);
        Assert.Equal(4, report.WinPointSums.Length);
        Assert.Equal(4, report.RankCounts.Length);
        foreach (var row in report.RankCounts)
        {
            Assert.Equal(4, row.Length);
        }
    }

    [Fact]
    public void 対局未完了時の平均順位は0()
    {
        // Arrange
        var tracer = new StatsTracer();

        // Act
        var report = tracer.Build();

        // Assert
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(0, report.AverageRank(i));
        }
    }
}
