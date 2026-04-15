using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 和了による局終了イベント
/// </summary>
/// <param name="Winners">和了者 (複数はダブロン/トリプルロン)</param>
public record GameEventRoundEndedByWin(ImmutableArray<PlayerIndex> Winners) : GameEvent
{
    public override string Name => "局終了(和了)";
}
