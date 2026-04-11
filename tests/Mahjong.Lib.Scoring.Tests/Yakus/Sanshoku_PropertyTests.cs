using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Sanshoku_PropertyTests
{
    [Fact]
    public void Number_25を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshoku;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public void Name_三色同順を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshoku;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("三色同順", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshoku;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sanshoku;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Sanshoku;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
