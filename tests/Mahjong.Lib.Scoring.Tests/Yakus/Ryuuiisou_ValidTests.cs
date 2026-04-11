using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Ryuuiisou_ValidTests
{
    [Fact]
    public void Valid_緑の牌のみの場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(sou: "222"), new(sou: "333"), new(sou: "444"), new(sou: "666"), new(honor: "rr")]);
        var callList = new CallList();

        // Act
        var actual = Ryuuiisou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_緑の牌のみで副露を含む場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(sou: "222"), new(sou: "333"), new(honor: "rr")]);
        var callList = new CallList([Call.Pon(new(sou: "444")), Call.Chi(new(sou: "234"))]);

        // Act
        var actual = Ryuuiisou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_緑以外の牌が含まれる場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(sou: "222"), new(sou: "333"), new(sou: "444"), new(sou: "666"), new(sou: "55")]);
        var callList = new CallList();

        // Act
        var actual = Ryuuiisou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_緑以外の牌が副露に含まれる場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(sou: "222"), new(sou: "333"), new(honor: "rr")]);
        var callList = new CallList([Call.Pon(new(sou: "444")), Call.Chi(new(man: "123"))]);

        // Act
        var actual = Ryuuiisou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
