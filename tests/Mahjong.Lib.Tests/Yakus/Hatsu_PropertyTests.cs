using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Hatsu_PropertyTests
{
    [Fact]
    public void Number_19を返す()
    {
        // Arrange
        var yaku = Yaku.Hatsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(19, result);
    }

    [Fact]
    public void Name_發を返す()
    {
        // Arrange
        var yaku = Yaku.Hatsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("發", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Hatsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Hatsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Hatsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
