using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class DaisuushiiDouble_ValidTests
{
    [Fact]
    public void Valid_ダブル役満有効_大四喜の条件を満たす_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(honor: "ttt"), new(honor: "sss"), new(honor: "ppp")]);
        var callList = new CallList([Call.Pon(new(honor: "nnn"))]);
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = DaisuushiiDouble.Valid(hand, callList, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ダブル役満無効_大四喜の条件を満たす_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(honor: "ttt"), new(honor: "sss"), new(honor: "ppp")]);
        var callList = new CallList([Call.Pon(new(honor: "nnn"))]);
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = DaisuushiiDouble.Valid(hand, callList, gameRules);

        // Assert
        Assert.False(actual);
    }
}
