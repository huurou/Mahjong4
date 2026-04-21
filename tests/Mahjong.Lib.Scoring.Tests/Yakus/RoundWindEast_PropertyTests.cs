using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class RoundWindEast_PropertyTests
{
    [Fact]
    public void Number_14を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindEast;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(14, result);
    }

    [Fact]
    public void Name_場風東を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindEast;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("場風 東", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindEast;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindEast;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.RoundWindEast;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
