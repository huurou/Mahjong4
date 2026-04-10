using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Chinroutou_ValidTests
{

    [Fact]
    public void Valid_全ての牌が老頭牌_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "999"), new(sou: "11")]);
        var callList = new CallList([Call.Pon(new(sou: "999")), Call.Pon(new(man: "999"))]);

        // Act
        var actual = Chinroutou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_中張牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "999"), new(sou: "11")]);
        var callList = new CallList([Call.Pon(new(sou: "555")), Call.Pon(new(man: "999"))]);

        // Act
        var actual = Chinroutou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "999"), new(sou: "11")]);
        var callList = new CallList([Call.Pon(new(honor: "ccc")), Call.Pon(new(man: "999"))]);

        // Act
        var actual = Chinroutou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
