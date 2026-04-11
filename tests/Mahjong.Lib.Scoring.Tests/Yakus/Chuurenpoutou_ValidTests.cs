using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chuurenpoutou_ValidTests
{
    [Fact]
    public void Valid_同一スートで1112345678999と任意の数牌_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "234"), new(man: "567"), new(man: "88"), new(man: "999")]);

        // Act
        var actual = Chuurenpoutou.Valid(hand);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_同一スートで1112345678999と任意の数牌_別パターン_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "111"), new(pin: "234"), new(pin: "567"), new(pin: "789"), new(pin: "99")]);

        // Act
        var actual = Chuurenpoutou.Valid(hand);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_複数のスートが混在_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "234"), new(man: "567"), new(man: "88"), new(pin: "999")]);

        // Act
        var actual = Chuurenpoutou.Valid(hand);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同一スートだが九蓮宝燈の形でない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(sou: "111"), new(sou: "234"), new(sou: "345"), new(sou: "789"), new(sou: "99")]);

        // Act
        var actual = Chuurenpoutou.Valid(hand);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "234"), new(man: "567"), new(man: "789"), new(honor: "tt")]);

        // Act
        var actual = Chuurenpoutou.Valid(hand);

        // Assert
        Assert.False(actual);
    }
}
