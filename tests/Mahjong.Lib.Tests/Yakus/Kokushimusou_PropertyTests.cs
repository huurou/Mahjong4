using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Kokushimusou_PropertyTests
{
    [Fact]
    public void Number_47を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(47, result);
    }

    [Fact]
    public void Name_国士無双を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("国士無双", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
