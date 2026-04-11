using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Shousangen_PropertyTests
{
    [Fact]
    public void Number_30を返す()
    {
        // Arrange
        var yaku = Yaku.Shousangen;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(30, result);
    }

    [Fact]
    public void Name_小三元を返す()
    {
        // Arrange
        var yaku = Yaku.Shousangen;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("小三元", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Shousangen;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Shousangen;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Shousangen;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
