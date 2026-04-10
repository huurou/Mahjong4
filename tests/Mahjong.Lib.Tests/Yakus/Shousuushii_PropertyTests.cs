using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Shousuushii_PropertyTests
{
    [Fact]
    public void Number_50を返す()
    {
        // Arrange
        var yaku = Yaku.Shousuushii;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(50, result);
    }

    [Fact]
    public void Name_小四喜を返す()
    {
        // Arrange
        var yaku = Yaku.Shousuushii;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("小四喜", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Shousuushii;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Shousuushii;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Shousuushii;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
