using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Ippatsu_PropertyTests
{
    [Fact]
    public void Number_2を返す()
    {
        // Arrange
        var yaku = Yaku.Ippatsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void Name_一発を返す()
    {
        // Arrange
        var yaku = Yaku.Ippatsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("一発", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Ippatsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_1を返す()
    {
        // Arrange
        var yaku = Yaku.Ippatsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Ippatsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
