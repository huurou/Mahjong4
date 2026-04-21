using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Adoptions;

public class AdoptedRyuukyokuAction_ConstructorTests
{
    [Fact]
    public void 荒牌平局_全フィールドが保持される()
    {
        // Arrange
        var tenpaiPlayers = ImmutableList.Create(new PlayerIndex(0), new PlayerIndex(2));
        var nagashiPlayers = ImmutableList<PlayerIndex>.Empty;
        var pointDeltas = new PointArray(new Point(0))
            .AddPoint(new PlayerIndex(0), 1500)
            .AddPoint(new PlayerIndex(2), 1500)
            .SubtractPoint(new PlayerIndex(1), 1500)
            .SubtractPoint(new PlayerIndex(3), 1500);

        // Act
        var action = new AdoptedRyuukyokuAction(
            RyuukyokuType.KouhaiHeikyoku,
            tenpaiPlayers,
            nagashiPlayers,
            pointDeltas,
            true
        );

        // Assert
        Assert.Equal(RyuukyokuType.KouhaiHeikyoku, action.Type);
        Assert.Equal(2, action.TenpaiPlayerIndices.Count);
        Assert.Empty(action.NagashiManganPlayerIndices);
        Assert.Equal(pointDeltas, action.PointDeltas);
        Assert.True(action.DealerContinues);
    }

    [Fact]
    public void 途中流局_AdoptedRoundActionを継承している()
    {
        // Act
        AdoptedRoundAction action = new AdoptedRyuukyokuAction(
            RyuukyokuType.SuuchaRiichi,
            [],
            [],
            new PointArray(new Point(0)),
            true
        );

        // Assert
        Assert.IsType<AdoptedRoundAction>(action, exactMatch: false);
    }
}
