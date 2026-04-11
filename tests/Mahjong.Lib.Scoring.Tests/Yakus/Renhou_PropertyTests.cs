using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Renhou_PropertyTests
{
    [Fact]
    public void Number_36を返す()
    {
        // Arrange
        var yaku = Yaku.Renhou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(36, result);
    }

    [Fact]
    public void Name_人和を返す()
    {
        // Arrange
        var yaku = Yaku.Renhou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("人和", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Renhou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_5を返す()
    {
        // Arrange
        var yaku = Yaku.Renhou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Renhou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
