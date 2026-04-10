using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Sankantsu_PropertyTests
{
    [Fact]
    public void Number_27を返す()
    {
        // Arrange
        var yaku = Yaku.Sankantsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(27, result);
    }

    [Fact]
    public void Name_三槓子を返す()
    {
        // Arrange
        var yaku = Yaku.Sankantsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("三槓子", result);
    }

    [Fact]
    public void HanOpen_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sankantsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Sankantsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Sankantsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
