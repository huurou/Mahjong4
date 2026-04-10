using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Junchan_ValidTests
{
    [Fact]
    public void Valid_正常形_副露なし_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "789"), new(pin: "11"), new(sou: "789"), new(pin: "123")]);
        var callList = new CallList();

        // Act
        var actual = Junchan.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_正常形_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "11"), new(sou: "789")]);
        var callList = new CallList([
            Call.Chi(new (man: "789")),
            Call.Pon(new (pin: "999")),
        ]);

        // Act
        var actual = Junchan.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_順子なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "999"), new(pin: "11"), new(sou: "999"), new(pin: "111")]);
        var callList = new CallList();

        // Act
        var actual = Junchan.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_么九牌を含まない面子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(pin: "11"), new(sou: "789"), new(pin: "123")]);
        var callList = new CallList();

        // Act
        var actual = Junchan.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "789"), new(pin: "11"), new(sou: "123"), new(honor: "ttt")]);
        var callList = new CallList();

        // Act
        var actual = Junchan.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
