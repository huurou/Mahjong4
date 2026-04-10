using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Chanta_ValidTests
{
    [Fact]
    public void Valid_正常形_副露なし_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "789"), new(pin: "11"), new(sou: "789"), new(honor: "hhh")]);
        var callList = new CallList();

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_正常形_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "11"), new(sou: "789")]);
        var callList = new CallList([Call.Chi(new(man: "789")), Call.Pon(new(honor: "hhh"))]);

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_順子なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "999"), new(pin: "11"), new(sou: "999"), new(honor: "hhh")]);
        var callList = new CallList();

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_么九牌を含まない面子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(pin: "11"), new(sou: "789"), new(honor: "hhh")]);
        var callList = new CallList();

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_么九牌のみで字牌なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "789"), new(pin: "11"), new(sou: "123"), new(sou: "789")]);
        var callList = new CallList();

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌のみで么九牌なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(honor: "ttt"), new(honor: "nnn"), new(honor: "sss"), new(honor: "ppp"), new(honor: "hh")]);
        var callList = new CallList();

        // Act
        var actual = Chanta.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
