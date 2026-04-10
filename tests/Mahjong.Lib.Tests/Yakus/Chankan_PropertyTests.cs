using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Chankan_PropertyTests
{
    [Fact]
    public void Number_3を返す()
    {
        // Arrange
        var yaku = Yaku.Chankan;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void Name_槍槓を返す()
    {
        // Arrange
        var yaku = Yaku.Chankan;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("槍槓", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Chankan;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Chankan;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Chankan;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
