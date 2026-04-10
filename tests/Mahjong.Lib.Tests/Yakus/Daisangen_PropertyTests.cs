using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Daisangen_PropertyTests
{
    [Fact]
    public void Number_39を返す()
    {
        // Arrange
        var yaku = Yaku.Daisangen;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(39, result);
    }

    [Fact]
    public void Name_大三元を返す()
    {
        // Arrange
        var yaku = Yaku.Daisangen;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("大三元", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Daisangen;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Daisangen;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Daisangen;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
