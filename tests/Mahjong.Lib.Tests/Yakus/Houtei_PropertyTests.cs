using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Houtei_PropertyTests
{
    [Fact]
    public void Number_6を返す()
    {
        // Arrange
        var yaku = Yaku.Houtei;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(6, result);
    }

    [Fact]
    public void Name_河底撈魚を返す()
    {
        // Arrange
        var yaku = Yaku.Houtei;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("河底撈魚", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Houtei;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Houtei;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Houtei;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
