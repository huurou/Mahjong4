using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Shousangen_ValidTests
{
    [Fact]
    public void Valid_三元牌の対子と2組の刻子_手牌のみ_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(pin: "456"), new(honor: "hh"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_三元牌の対子と2組の刻子_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(pin: "456"), new(honor: "hh")]);
        var callList = new CallList([Call.Pon(new(honor: "rrr")), Call.Pon(new(honor: "ccc"))]);

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_対子が三元牌で手牌と副露に三元牌の刻子が1種ずつある場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(pin: "456"), new(honor: "hh"), new(honor: "rrr")]);
        var callList = new CallList([Call.Minkan(new(honor: "cccc"))]);

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_三元牌の刻子が3組_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "hhh"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_三元牌の刻子が1組と対子が1組_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(pin: "234"), new(pin: "567"), new(pin: "789"), new(honor: "hh")]);
        var callList = new CallList([Call.Pon(new(honor: "rrr"))]);

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_三元牌以外の対子_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(man: "234"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_対子がない手牌_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(man: "456"), new(man: "789"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList();

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_副露に三元牌以外のポンがある場合_三元牌の刻子が2組なら成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "123"), new(honor: "hh"), new(honor: "rrr")]);
        var callList = new CallList([Call.Pon(honor: "ccc"), Call.Pon(sou: "999")]);

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_副露にチーがある場合_三元牌の刻子が2組と対子で成立する()
    {
        // Arrange
        var hand = new Hand([new(honor: "hh"), new(honor: "rrr"), new(honor: "ccc")]);
        var callList = new CallList([Call.Chi(pin: "123"), Call.Chi(sou: "789")]);

        // Act
        var actual = Shousangen.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }
}
