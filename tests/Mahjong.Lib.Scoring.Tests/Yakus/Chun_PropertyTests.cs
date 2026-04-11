using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chun_PropertyTests
{
    [Fact]
    public void Number_20を返す()
    {
        // Arrange
        var yaku = Yaku.Chun;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void Name_中を返す()
    {
        // Arrange
        var yaku = Yaku.Chun;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("中", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Chun;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Chun;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Chun;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
