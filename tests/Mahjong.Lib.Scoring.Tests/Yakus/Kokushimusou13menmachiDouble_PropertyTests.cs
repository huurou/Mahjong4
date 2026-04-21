using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Kokushimusou13menmachiDouble_PropertyTests
{
    [Fact]
    public void Number_48を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachiDouble;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(48, result);
    }

    [Fact]
    public void Name_国士無双１３面を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachiDouble;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("国士無双１３面", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachiDouble;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_26を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachiDouble;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachiDouble;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
