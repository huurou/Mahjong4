using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Ryuuiisou_PropertyTests
{
    [Fact]
    public void Number_43を返す()
    {
        // Arrange
        var yaku = Yaku.Ryuuiisou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(43, result);
    }

    [Fact]
    public void Name_緑一色を返す()
    {
        // Arrange
        var yaku = Yaku.Ryuuiisou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("緑一色", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Ryuuiisou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Ryuuiisou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Ryuuiisou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
