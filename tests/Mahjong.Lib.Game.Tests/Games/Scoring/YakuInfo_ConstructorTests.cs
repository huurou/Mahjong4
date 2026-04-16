using Mahjong.Lib.Game.Games.Scoring;

namespace Mahjong.Lib.Game.Tests.Games.Scoring;

public class YakuInfo_ConstructorTests
{
    [Fact]
    public void 通常役_全フィールドが保持される()
    {
        // Act
        var yakuInfo = new YakuInfo(0, "平和", 1);

        // Assert
        Assert.Equal(0, yakuInfo.Number);
        Assert.Equal("平和", yakuInfo.Name);
        Assert.Equal(1, yakuInfo.Han);
        Assert.Equal(0, yakuInfo.YakumanCount);
    }

    [Fact]
    public void 役満_Hanがnullで役満倍数が1()
    {
        // Act
        var yakuInfo = new YakuInfo(37, "国士無双", null, 1);

        // Assert
        Assert.Equal(37, yakuInfo.Number);
        Assert.Null(yakuInfo.Han);
        Assert.Equal(1, yakuInfo.YakumanCount);
    }

    [Fact]
    public void ダブル役満_役満倍数が2()
    {
        // Act
        var yakuInfo = new YakuInfo(50, "四暗刻単騎", null, 2);

        // Assert
        Assert.Equal(2, yakuInfo.YakumanCount);
    }
}
