using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Junchan_PropertyTests
{
    [Fact]
    public void Number_33を返す()
    {
        // Arrange
        var yaku = Yaku.Junchan;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(33, result);
    }

    [Fact]
    public void Name_純全帯么九を返す()
    {
        // Arrange
        var yaku = Yaku.Junchan;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("純全帯么九", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Junchan;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_3を返す()
    {
        // Arrange
        var yaku = Yaku.Junchan;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Junchan;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
