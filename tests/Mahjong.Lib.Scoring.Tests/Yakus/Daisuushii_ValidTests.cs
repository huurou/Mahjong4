using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Daisuushii_ValidTests
{
    [Fact]
    public void Valid_風牌4種類_刻子_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "ttt"), new(honor: "sss")]);
        var callList = new CallList([
            Call.Pon(new TileKindList(honor: "nnn")),
            Call.Pon(new TileKindList(honor: "ppp")),
        ]);

        // Act
        var actual = Daisuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌4種類_槓子_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11")]);
        var callList = new CallList([
            Call.Ankan(new TileKindList(honor: "tttt")),
            Call.Minkan(new TileKindList(honor: "ssss")),
            Call.Minkan(new TileKindList(honor: "nnnn")),
            Call.Ankan(new TileKindList(honor: "pppp")),
        ]);

        // Act
        var actual = Daisuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌4種類_刻子と槓子の混合_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "ttt"), new(honor: "sss")]);
        var callList = new CallList([
            Call.Minkan(new TileKindList(honor: "nnnn")),
            Call.Ankan(new TileKindList(honor: "pppp")),
        ]);

        // Act
        var actual = Daisuushii.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌の刻子が3つだけ_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(honor: "ttt"), new(honor: "sss"), new(pin: "234")]);
        var callList = new CallList([Call.Pon(new TileKindList(honor: "nnn"))]);

        // Act
        var actual = Daisuushii.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
