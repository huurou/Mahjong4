using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Sanankou_PropertyTests
{
    [Fact]
    public void Number_29を返す()
    {
        // Arrange
        var yaku = Yaku.Sanankou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(29, result);
    }

    [Fact]
    public void Name_三暗刻を返す()
    {
        // Arrange
        var yaku = Yaku.Sanankou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("三暗刻", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sanankou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sanankou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Sanankou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
