using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Tsuuiisou_PropertyTests
{
    [Fact]
    public void Number_42を返す()
    {
        // Arrange
        var yaku = Yaku.Tsuuiisou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Name_字一色を返す()
    {
        // Arrange
        var yaku = Yaku.Tsuuiisou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("字一色", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Tsuuiisou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Tsuuiisou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Tsuuiisou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
