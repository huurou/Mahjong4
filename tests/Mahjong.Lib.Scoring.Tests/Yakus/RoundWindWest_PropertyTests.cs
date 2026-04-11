using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class RoundWindWest_PropertyTests
{
    [Fact]
    public void Number_16を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindWest;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(16, result);
    }

    [Fact]
    public void Name_場風牌西を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindWest;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("場風牌・西", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindWest;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindWest;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindWest;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
