using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Chun_ValidTests
{

    [Fact]
    public void Valid_中の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Chun.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_中の明槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Minkan(new TileKindList(honor: "cccc"))]);

        // Act
        var actual = Chun.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_中の刻子なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "hhh")]);
        var callList = new CallList();

        // Act
        var actual = Chun.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
