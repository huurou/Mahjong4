using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Tanyao_PropertyTests
{
    [Fact]
    public void Number_8を返す()
    {
        // Arrange
        var yaku = Yaku.Tanyao;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void Name_断么九を返す()
    {
        // Arrange
        var yaku = Yaku.Tanyao;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("断么九", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Tanyao;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Tanyao;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Tanyao;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
