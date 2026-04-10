using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Ittsuu_ValidTests
{

    [Fact]
    public void Valid_手牌のみで萬子の一気通貫_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "789"), new(pin: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_手牌のみで筒子の一気通貫_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(pin: "456"), new(pin: "789"), new(man: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_手牌のみで索子の一気通貫_成立する()
    {
        // Arrange
        var hand = new Hand([new(sou: "123"), new(sou: "456"), new(sou: "789"), new(man: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_手牌と副露で一気通貫_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(honor: "tt"), new(pin: "111")]);
        var callList = new CallList([Call.Chi(new(man: "789"))]);

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_123も456も789も揃っているが異なる種類_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "456"), new(sou: "789"), new(man: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同一種類だが123と456と789ではない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "456"), new(man: "789"), new(pin: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_123と456と789が揃わない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "345"), new(man: "789"), new(pin: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_順子が3つない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "111"), new(pin: "111"), new(honor: "tt")]);
        var callList = new CallList();

        // Act
        var actual = Ittsuu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
