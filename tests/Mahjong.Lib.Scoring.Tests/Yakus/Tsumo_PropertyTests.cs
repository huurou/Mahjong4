using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Tsumo_PropertyTests
{
    [Fact]
    public void Number_0を返す()
    {
        // Arrange
        var yaku = Yaku.Tsumo;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Name_門前清自摸和を返す()
    {
        // Arrange
        var yaku = Yaku.Tsumo;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("門前清自摸和", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Tsumo;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Tsumo;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Tsumo;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
