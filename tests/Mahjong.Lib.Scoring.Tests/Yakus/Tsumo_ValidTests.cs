using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Tsumo_ValidTests
{
    [Fact]
    public void Valid_門前_ツモ和了_成立する()
    {
        // Arrange
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Tsumo.Valid(callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_門前_ロン和了_成立しない()
    {
        // Arrange
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = Tsumo.Valid(callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_副露あり_ツモ和了_成立しない()
    {
        // Arrange
        var callList = new CallList([Call.Chi(new(sou: "345"))]);
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Tsumo.Valid(callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
