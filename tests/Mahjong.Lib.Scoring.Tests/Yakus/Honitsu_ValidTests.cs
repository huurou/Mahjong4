using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Honitsu_ValidTests
{
    [Fact]
    public void Valid_萬子と字牌_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "789"), new(man: "99"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Honitsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_筒子と字牌_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(pin: "456"), new(pin: "789"), new(pin: "99")]);
        var callList = new CallList([Call.Minkan(new TileKindList(honor: "hhhh"))]);

        // Act
        var actual = Honitsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_索子と字牌_成立する()
    {
        // Arrange
        var hand = new Hand([new(sou: "11"), new(sou: "456"), new(sou: "789")]);
        var callList = new CallList([
            Call.Pon(new (honor: "ccc")),
            Call.Minkan(new (sou: "1111")),
        ]);

        // Act
        var actual = Honitsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_複数の数牌_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "456"), new(sou: "789"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Honitsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌なし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "789"), new(man: "999"), new(man: "11")]);
        var callList = new CallList();

        // Act
        var actual = Honitsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
