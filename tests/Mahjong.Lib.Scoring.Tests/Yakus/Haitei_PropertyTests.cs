using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Haitei_PropertyTests
{
    [Fact]
    public void Number_5を返す()
    {
        // Arrange
        var yaku = Yaku.Haitei;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Name_海底摸月を返す()
    {
        // Arrange
        var yaku = Yaku.Haitei;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("海底摸月", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Haitei;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Haitei;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Haitei;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
