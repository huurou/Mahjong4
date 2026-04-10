using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class RoundWindSouth_PropertyTests
{
    [Fact]
    public void Number_15を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindSouth;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void Name_場風牌南を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindSouth;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("場風牌・南", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindSouth;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindSouth;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindSouth;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
