using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Ittsuu_PropertyTests
{
    [Fact]
    public void Number_24を返す()
    {
        // Arrange
        var yaku = Yaku.Ittsuu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(24, result);
    }

    [Fact]
    public void Name_一気通貫を返す()
    {
        // Arrange
        var yaku = Yaku.Ittsuu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("一気通貫", result);
    }

    [Fact]
    public void HanOpen_1を返す()
    {
        // Arrange
        var yaku = Yaku.Ittsuu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Ittsuu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Ittsuu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
