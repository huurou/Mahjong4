using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Ryanpeikou_PropertyTests
{
    [Fact]
    public void Number_32を返す()
    {
        // Arrange
        var yaku = Yaku.Ryanpeikou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(32, result);
    }

    [Fact]
    public void Name_二盃口を返す()
    {
        // Arrange
        var yaku = Yaku.Ryanpeikou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("二盃口", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Ryanpeikou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_3を返す()
    {
        // Arrange
        var yaku = Yaku.Ryanpeikou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Ryanpeikou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
