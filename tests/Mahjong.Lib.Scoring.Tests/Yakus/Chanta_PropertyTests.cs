using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chanta_PropertyTests
{
    [Fact]
    public void Number_23を返す()
    {
        // Arrange
        var yaku = Yaku.Chanta;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(23, result);
    }

    [Fact]
    public void Name_混全帯么九を返す()
    {
        // Arrange
        var yaku = Yaku.Chanta;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("混全帯么九", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Chanta;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Chanta;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Chanta;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
