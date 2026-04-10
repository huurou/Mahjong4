using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Ryanpeikou_ValidTests
{
    [Fact]
    public void Valid_同じ順子が2組あり_面前の場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "123"), new(pin: "456"), new(pin: "456"), new(sou: "11")]);
        var callList = new CallList();

        // Act
        var actual = Ryanpeikou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_同じ順子が2組未満_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "123"), new(pin: "456"), new(pin: "678"), new(sou: "11")]);
        var callList = new CallList();

        // Act
        var actual = Ryanpeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同じ順子が2組あるが_副露がある場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "123"), new(pin: "456"), new(sou: "11")]);
        var callList = new CallList([Call.Chi(new(pin: "456"))]);

        // Act
        var actual = Ryanpeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同じスーツの連続した順子_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "234"), new(pin: "123"), new(pin: "234"), new(sou: "11")]);
        var callList = new CallList();

        // Act
        var actual = Ryanpeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
