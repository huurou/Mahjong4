using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class PlayerWindWest_PropertyTests
{
    [Fact]
    public void Number_12を返す()
    {
        // Arrange
        var yaku = Yaku.PlayerWindWest;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(12, result);
    }

    [Fact]
    public void Name_自風西を返す()
    {
        // Arrange
        var yaku = Yaku.PlayerWindWest;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("自風 西", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.PlayerWindWest;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.PlayerWindWest;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.PlayerWindWest;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
