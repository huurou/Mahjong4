using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Daisharin_PropertyTests
{
    [Fact]
    public void Number_56を返す()
    {
        // Arrange
        var yaku = Yaku.Daisharin;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(56, result);
    }

    [Fact]
    public void Name_大車輪を返す()
    {
        // Arrange
        var yaku = Yaku.Daisharin;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("大車輪", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Daisharin;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Daisharin;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Daisharin;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
