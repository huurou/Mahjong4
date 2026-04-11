using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Suukantsu_PropertyTests
{
    [Fact]
    public void Number_51を返す()
    {
        // Arrange
        var yaku = Yaku.Suukantsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(51, result);
    }

    [Fact]
    public void Name_四槓子を返す()
    {
        // Arrange
        var yaku = Yaku.Suukantsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("四槓子", result);
    }

    [Fact]
    public void HanOpen_13を返す()
    {
        // Arrange
        var yaku = Yaku.Suukantsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Suukantsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Suukantsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
