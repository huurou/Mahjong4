using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Chuurenpoutou_PropertyTests
{
    [Fact]
    public void Number_45を返す()
    {
        // Arrange
        var yaku = Yaku.Chuurenpoutou;

        // Act
        var result = yaku.Number;

        // Assert
        Assert.Equal(45, result);
    }

    [Fact]
    public void Name_九蓮宝燈を返す()
    {
        // Arrange
        var yaku = Yaku.Chuurenpoutou;

        // Act
        var result = yaku.Name;

        // Assert
        Assert.Equal("九蓮宝燈", result);
    }

    [Fact]
    public void HanOpen_0を返す()
    {
        // Arrange
        var yaku = Yaku.Chuurenpoutou;

        // Act
        var result = yaku.HanOpen;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HanClosed_13を返す()
    {
        // Arrange
        var yaku = Yaku.Chuurenpoutou;

        // Act
        var result = yaku.HanClosed;

        // Assert
        Assert.Equal(13, result);
    }

    [Fact]
    public void IsYakuman_trueを返す()
    {
        // Arrange
        var yaku = Yaku.Chuurenpoutou;

        // Act
        var result = yaku.IsYakuman;

        // Assert
        Assert.True(result);
    }
}
