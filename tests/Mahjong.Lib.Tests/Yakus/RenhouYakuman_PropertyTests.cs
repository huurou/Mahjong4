using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class RenhouYakuman_PropertyTests
{
    [Fact]
    public void Number_36を返す()
    {
        // Arrange
        var yaku = Yaku.RenhouYakuman;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(36, result);
    }

    [Fact]
    public void Name_人和を返す()
    {
        // Arrange
        var yaku = Yaku.RenhouYakuman;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("人和", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.RenhouYakuman;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.RenhouYakuman;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.RenhouYakuman;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
