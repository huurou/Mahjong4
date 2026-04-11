using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Nagashimangan_PropertyTests
{
    [Fact]
    public void Number_55を返す()
    {
        // Arrange
        var yaku = Yaku.Nagashimangan;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(55, result);
    }

    [Fact]
    public void Name_流し満貫を返す()
    {
        // Arrange
        var yaku = Yaku.Nagashimangan;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("流し満貫", result);
    }

    [Fact]
    public void HanOpen_5を返す()
    {
        // Arrange
        var yaku = Yaku.Nagashimangan;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void HanClosed_5を返す()
    {
        // Arrange
        var yaku = Yaku.Nagashimangan;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Nagashimangan;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
