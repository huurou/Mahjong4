using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class JunseiChuurenpoutouDouble_PropertyTests
{
    [Fact]
    public void Number_46を返す()
    {
        // Arrange
        var yaku = Yaku.JunseiChuurenpoutouDouble;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(46, result);
    }

    [Fact]
    public void Name_純正九蓮宝燈を返す()
    {
        // Arrange
        var yaku = Yaku.JunseiChuurenpoutouDouble;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("純正九蓮宝燈", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.JunseiChuurenpoutouDouble;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_26を返す()
    {
        // Arrange
        var yaku = Yaku.JunseiChuurenpoutouDouble;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.JunseiChuurenpoutouDouble;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
