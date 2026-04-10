using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class DaisuushiiDouble_PropertyTests
{
    [Fact]
    public void Number_49を返す()
    {
        // Arrange
        var yaku = Yaku.DaisuushiiDouble;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(49, result);
    }

    [Fact]
    public void Name_大四喜を返す()
    {
        // Arrange
        var yaku = Yaku.DaisuushiiDouble;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("大四喜", result);
    }

    [Fact]
    public void HanOpen_26を返す()
    {
        // Arrange
        var yaku = Yaku.DaisuushiiDouble;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void HanClosed_26を返す()
    {
        // Arrange
        var yaku = Yaku.DaisuushiiDouble;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.DaisuushiiDouble;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
