using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Chiitoitsu_PropertyTests
{
    [Fact]
    public void Number_22を返す()
    {
        // Arrange
        var yaku = Yaku.Chiitoitsu;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(22, result);
    }

    [Fact]
    public void Name_七対子を返す()
    {
        // Arrange
        var yaku = Yaku.Chiitoitsu;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("七対子", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Chiitoitsu;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_2を返す()
    {
        // Arrange
        var yaku = Yaku.Chiitoitsu;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void IsYakuman_falseを返す()
    {
        // Arrange
        var yaku = Yaku.Chiitoitsu;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.False(result);
    }
}
