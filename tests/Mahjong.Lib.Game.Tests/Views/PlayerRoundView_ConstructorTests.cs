using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Views;

public class PlayerRoundView_ConstructorTests
{
    [Fact]
    public void 全フィールドが保持される()
    {
        // Arrange
        var selfIndex = new PlayerIndex(0);
        var ownHand = new Hand();
        var ownStatus = new OwnRoundStatus(true, false, true, true, false, false, false);
        var otherStatuses = ImmutableArray.Create(
            new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true),
            new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true),
            new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true)
        );
        var doraIndicators = ImmutableList.Create(new Tile(5));

        // Act
        var view = new PlayerRoundView(
            selfIndex,
            RoundWind.East,
            new RoundNumber(0),
            new Honba(1),
            new KyoutakuRiichiCount(2),
            new PlayerIndex(0),
            new PointArray(new Point(35000)),
            ownHand,
            new CallListArray(),
            new RiverArray(),
            doraIndicators,
            ownStatus,
            otherStatuses,
            70
        );

        // Assert
        Assert.Equal(selfIndex, view.ViewerIndex);
        Assert.Equal(RoundWind.East, view.RoundWind);
        Assert.Equal(0, view.RoundNumber.Value);
        Assert.Equal(1, view.Honba.Value);
        Assert.Equal(2, view.KyoutakuRiichiCount.Value);
        Assert.True(view.OwnStatus.IsRiichi);
        Assert.True(view.OwnStatus.IsIppatsu);
        Assert.False(view.OwnStatus.IsFuriten);
        Assert.Equal(3, view.OtherPlayerStatuses.Length);
        Assert.Single(view.DoraIndicators);
        Assert.Equal(70, view.WallLiveRemaining);
    }
}
