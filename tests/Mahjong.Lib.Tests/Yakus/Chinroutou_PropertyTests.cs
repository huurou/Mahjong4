using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Chinroutou_PropertyTests
{
    [Fact]
    public void Number_44を返す()
    {
        // Arrange
        var yaku = Yaku.Chinroutou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(44, result);
    }

    [Fact]
    public void Name_清老頭を返す()
    {
        // Arrange
        var yaku = Yaku.Chinroutou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("清老頭", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Chinroutou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Chinroutou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Chinroutou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
