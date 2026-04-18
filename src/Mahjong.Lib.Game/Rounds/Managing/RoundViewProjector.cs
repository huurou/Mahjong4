using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// Round から PlayerRoundView を生成する既定実装
/// </summary>
public sealed class RoundViewProjector : IRoundViewProjector
{
    public PlayerRoundView Project(Round round, PlayerIndex viewerIndex)
    {
        ArgumentNullException.ThrowIfNull(round);

        var ownPlayerStatus = round.PlayerRoundStatusArray[viewerIndex];
        var ownStatus = new OwnRoundStatus(
            IsRiichi: ownPlayerStatus.IsRiichi,
            IsDoubleRiichi: ownPlayerStatus.IsDoubleRiichi,
            IsIppatsu: ownPlayerStatus.IsIppatsu,
            IsMenzen: ownPlayerStatus.IsMenzen,
            IsFuriten: ownPlayerStatus.IsFuriten,
            IsTemporaryFuriten: ownPlayerStatus.IsTemporaryFuriten,
            IsNagashiMangan: ownPlayerStatus.IsNagashiMangan
        );

        var otherStatuses = ImmutableArray.CreateBuilder<VisiblePlayerRoundStatus>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (playerIndex == viewerIndex) { continue; }
            var status = round.PlayerRoundStatusArray[playerIndex];
            otherStatuses.Add(new VisiblePlayerRoundStatus(
                PlayerIndex: playerIndex,
                IsRiichi: status.IsRiichi,
                IsDoubleRiichi: status.IsDoubleRiichi,
                IsMenzen: status.IsMenzen
            ));
        }

        var doraIndicators = ImmutableList.CreateBuilder<Tile>();
        for (var n = 0; n < round.Wall.DoraRevealedCount; n++)
        {
            doraIndicators.Add(round.Wall.GetDoraIndicator(n));
        }

        return new PlayerRoundView(
            ViewerIndex: viewerIndex,
            RoundWind: round.RoundWind,
            RoundNumber: round.RoundNumber,
            Honba: round.Honba,
            KyoutakuRiichiCount: round.KyoutakuRiichiCount,
            TurnIndex: round.Turn,
            PointArray: round.PointArray,
            OwnHand: round.HandArray[viewerIndex],
            CallListArray: round.CallListArray,
            RiverArray: round.RiverArray,
            DoraIndicators: doraIndicators.ToImmutable(),
            OwnStatus: ownStatus,
            OtherPlayerStatuses: otherStatuses.ToImmutable(),
            WallLiveRemaining: round.Wall.LiveRemaining
        );
    }
}
