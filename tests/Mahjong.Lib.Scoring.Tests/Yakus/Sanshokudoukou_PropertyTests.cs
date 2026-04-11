using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Sanshokudoukou_PropertyTests
{
    [Fact]
    public void Number_26を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshokudoukou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void Name_三色同刻を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshokudoukou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("三色同刻", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshokudoukou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshokudoukou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Sanshokudoukou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
