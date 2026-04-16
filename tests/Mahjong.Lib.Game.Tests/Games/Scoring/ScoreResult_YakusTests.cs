using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Games.Scoring;

public class ScoreResult_YakusTests
{
    [Fact]
    public void 空配列を渡す_空のYakuInfosが保持される()
    {
        // Act
        var result = new ScoreResult(3, 40, new PointArray(new Point(0)), []);

        // Assert
        Assert.Empty(result.YakuInfos);
    }

    [Fact]
    public void Yakus指定_役情報が保持される()
    {
        // Arrange
        var yakus = ImmutableList.Create(
            new YakuInfo(0, "平和", 1),
            new YakuInfo(1, "断么九", 1)
        );

        // Act
        var result = new ScoreResult(2, 30, new PointArray(new Point(0)), yakus);

        // Assert
        Assert.Equal(2, result.YakuInfos.Count);
        Assert.Equal("平和", result.YakuInfos[0].Name);
        Assert.Equal("断么九", result.YakuInfos[1].Name);
    }

    [Fact]
    public void YakuInfosに空リストを指定_既存フィールドが保持される()
    {
        // Act
        var result = new ScoreResult(1, 30, new PointArray(new Point(35000)), []);

        // Assert
        Assert.Equal(1, result.Han);
        Assert.Equal(30, result.Fu);
    }
}
