using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class DoubleRiichi_PropertyTests
{
    [Fact]
    public void Number_21を返す()
    {
        // Arrange
        var yaku = Yaku.DoubleRiichi;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(21, result);
    }

    [Fact]
    public void Name_両立直を返す()
    {
        // Arrange
        var yaku = Yaku.DoubleRiichi;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("両立直", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.DoubleRiichi;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.DoubleRiichi;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.DoubleRiichi;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
