using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Daisangen_ValidTests
{
    [Fact]
    public void Valid_白發中の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "hhh"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Daisangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_白發中の槓子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "hhh")]);
        var callList = new CallList([
            Call.Ankan(new TileKindList(honor: "rrrr")),
            Call.Minkan(new TileKindList(honor: "cccc"))
        ]);

        // Act
        var actual = Daisangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_白發中の刻子と槓子の混合_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "hhh"), new(honor: "rrr")]);
        var callList = new CallList([
            Call.Minkan(new TileKindList(honor: "cccc"))
        ]);

        // Act
        var actual = Daisangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_二色の三元牌の刻子と雀頭_成立しない()
    {
        // Arrange
        var hand = new Hand([new(honor: "hh"), new(honor: "rrr"), new(honor: "ccc"), new(pin: "234"), new(sou: "234")]);
        var callList = new CallList();

        // Act
        var actual = Daisangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_三元牌の刻子が一つだけ_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "hhh"), new(man: "234"), new(sou: "234"), new(pin: "456")]);
        var callList = new CallList();

        // Act
        var actual = Daisangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
