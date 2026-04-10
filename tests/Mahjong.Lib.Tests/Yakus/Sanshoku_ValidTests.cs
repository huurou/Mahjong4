using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Sanshoku_ValidTests
{

    [Fact]
    public void Valid_萬子筒子索子で同じ数の順子が揃う_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "123"), new(sou: "123"), new(man: "789"), new(pin: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshoku.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_萬子筒子索子で同じ数の順子が揃う_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "123"), new(pin: "55"), new(man: "789")]);
        var callList = new CallList([Call.Chi(new(sou: "123"))]);

        // Act
        var actual = Sanshoku.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_同じ数の順子が3種揃わない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "123"), new(sou: "456"), new(man: "789"), new(pin: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshoku.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同じ種類の牌のみ_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "789"), new(man: "33"), new(pin: "11")]);
        var callList = new CallList();

        // Act
        var actual = Sanshoku.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_順子が3つない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "123"), new(man: "111"), new(sou: "111"), new(pin: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshoku.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
