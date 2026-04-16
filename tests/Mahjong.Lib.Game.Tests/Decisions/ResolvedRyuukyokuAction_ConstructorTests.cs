using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Decisions;

public class ResolvedRyuukyokuAction_ConstructorTests
{
    [Fact]
    public void 荒牌平局_全フィールドが保持される()
    {
        // Arrange
        var tenpaiPlayers = ImmutableList.Create(new PlayerIndex(0), new PlayerIndex(2));
        var nagashiPlayers = ImmutableList<PlayerIndex>.Empty;

        // Act
        var action = new ResolvedRyuukyokuAction(
            RyuukyokuType.KouhaiHeikyoku,
            tenpaiPlayers,
            nagashiPlayers,
            true
        );

        // Assert
        Assert.Equal(RyuukyokuType.KouhaiHeikyoku, action.Type);
        Assert.Equal(2, action.TenpaiPlayerIndices.Count);
        Assert.Empty(action.NagashiManganPlayerIndices);
        Assert.True(action.DealerContinues);
    }

    [Fact]
    public void 途中流局_ResolvedRoundActionを継承している()
    {
        // Act
        ResolvedRoundAction action = new ResolvedRyuukyokuAction(
            RyuukyokuType.SuuchaRiichi,
            [],
            [],
            true
        );

        // Assert
        Assert.IsType<ResolvedRoundAction>(action, exactMatch: false);
    }
}
