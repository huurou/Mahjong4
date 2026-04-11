using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class SuuankouTankiDouble_PropertyTests
{
    [Fact]
    public void Number_41を返す()
    {
        // Arrange
        var yaku = Yaku.SuuankouTankiDouble;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(41, result);
    }

    [Fact]
    public void Name_四暗刻単騎待ちを返す()
    {
        // Arrange
        var yaku = Yaku.SuuankouTankiDouble;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("四暗刻単騎待ち", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.SuuankouTankiDouble;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_26を返す()
    {
        // Arrange
        var yaku = Yaku.SuuankouTankiDouble;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.SuuankouTankiDouble;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
