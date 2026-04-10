using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Riichi_PropertyTests
{
    [Fact]
    public void Number_1を返す()
    {
        // Arrange
        var yaku = Yaku.Riichi;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Name_立直を返す()
    {
        // Arrange
        var yaku = Yaku.Riichi;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("立直", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Riichi;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Riichi;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Riichi;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
