using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Honroutou_PropertyTests
{
    [Fact]
    public void Number_31を返す()
    {
        // Arrange
        var yaku = Yaku.Honroutou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(31, result);
    }

    [Fact]
    public void Name_混老頭を返す()
    {
        // Arrange
        var yaku = Yaku.Honroutou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("混老頭", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Honroutou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Honroutou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Honroutou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
