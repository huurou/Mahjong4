using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Chinitsu_PropertyTests
{
    [Fact]
    public void Number_35を返す()
    {
        // Arrange
        var yaku = Yaku.Chinitsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(35, result);
    }

    [Fact]
    public void Name_清一色を返す()
    {
        // Arrange
        var yaku = Yaku.Chinitsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("清一色", result);
    }

    [Fact]
    public void HanOpen_5を返す()
    {
        // Arrange
        var yaku = Yaku.Chinitsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void HanClosed_6を返す()
    {
        // Arrange
        var yaku = Yaku.Chinitsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(6, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Chinitsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
