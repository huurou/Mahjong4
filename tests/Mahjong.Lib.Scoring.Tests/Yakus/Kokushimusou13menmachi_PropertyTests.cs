using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Kokushimusou13menmachi_PropertyTests
{
    [Fact]
    public void Number_48を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachi;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(48, result);
    }

    [Fact]
    public void Name_国士無双十三面待ちを返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachi;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("国士無双十三面待ち", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachi;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachi;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Kokushimusou13menmachi;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
