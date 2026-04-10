using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Shousuushii_ValidTests
{
    [Fact]
    public void Valid_風牌の刻子3種と風牌の対子1種_手牌のみ_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(honor: "ttt"), new(honor: "sss"), new(honor: "nnn"), new(honor: "pp")]);
        var callList = new CallList();

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌の刻子3種と風牌の対子1種_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(honor: "ttt"), new(honor: "pp")]);
        var callList = new CallList([Call.Pon(new(honor: "sss")), Call.Pon(new(honor: "nnn"))]);

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌の刻子2種と槓子1種と風牌の対子1種_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(honor: "ttt"), new(honor: "pp")]);
        var callList = new CallList([Call.Pon(new(honor: "sss")), Call.Minkan(new(honor: "nnnn"))]);

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌の刻子4種_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "ttt"), new(honor: "sss"), new(honor: "nnn"), new(honor: "ppp")]);
        var callList = new CallList();

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_風牌の刻子2種と風牌の対子1種_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(pin: "234"), new(honor: "ttt"), new(honor: "nnn"), new(honor: "pp")]);
        var callList = new CallList();

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_風牌以外の対子_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "ttt"), new(honor: "nnn"), new(honor: "sss"), new(honor: "hhh")]);
        var callList = new CallList();

        // Act
        var actual = Shousuushii.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
