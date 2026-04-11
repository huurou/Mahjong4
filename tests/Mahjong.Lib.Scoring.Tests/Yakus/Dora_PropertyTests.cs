using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Dora_PropertyTests
{
    [Fact]
    public void Number_52を返す()
    {
        // Arrange
        var yaku = Yaku.Dora;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(52, result);
    }

    [Fact]
    public void Name_ドラを返す()
    {
        // Arrange
        var yaku = Yaku.Dora;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("ドラ", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Dora;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Dora;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Dora;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
