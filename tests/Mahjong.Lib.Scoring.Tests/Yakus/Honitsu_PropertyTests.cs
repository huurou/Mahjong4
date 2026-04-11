using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Honitsu_PropertyTests
{
    [Fact]
    public void Number_34を返す()
    {
        // Arrange
        var yaku = Yaku.Honitsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(34, result);
    }

    [Fact]
    public void Name_混一色を返す()
    {
        // Arrange
        var yaku = Yaku.Honitsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("混一色", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Honitsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_3を返す()
    {
        // Arrange
        var yaku = Yaku.Honitsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Honitsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
