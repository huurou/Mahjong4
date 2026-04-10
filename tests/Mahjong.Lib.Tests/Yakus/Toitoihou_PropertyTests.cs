using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Toitoihou_PropertyTests
{
    [Fact]
    public void Number_28を返す()
    {
        // Arrange
        var yaku = Yaku.Toitoihou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(28, result);
    }

    [Fact]
    public void Name_対々和を返す()
    {
        // Arrange
        var yaku = Yaku.Toitoihou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("対々和", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Toitoihou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Toitoihou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Toitoihou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
