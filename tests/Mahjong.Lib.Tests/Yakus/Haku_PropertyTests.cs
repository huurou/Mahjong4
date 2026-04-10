using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Haku_PropertyTests
{
    [Fact]
    public void Number_18を返す()
    {
        // Arrange
        var yaku = Yaku.Haku;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(18, result);
    }

    [Fact]
    public void Name_白を返す()
    {
        // Arrange
        var yaku = Yaku.Haku;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("白", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Haku;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Haku;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Haku;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
