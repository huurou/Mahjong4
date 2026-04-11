using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Tsuuiisou_ValidTests
{
    [Fact]
    public void Valid_字牌のみの場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(honor: "ttt"), new(honor: "sss"), new(honor: "ppp"), new(honor: "hhh"), new(honor: "rr")]);
        var callList = new CallList();

        // Act
        var actual = Tsuuiisou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_字牌のみで副露を含む場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(honor: "ttt"), new(honor: "hhh"), new(honor: "rr")]);
        var callList = new CallList([Call.Pon(new(honor: "sss")), Call.Pon(new(honor: "ppp"))]);

        // Act
        var actual = Tsuuiisou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_字牌以外が含まれる場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(honor: "ttt"), new(honor: "sss"), new(honor: "ppp"), new(man: "123"), new(honor: "rr")]);
        var callList = new CallList();

        // Act
        var actual = Tsuuiisou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
