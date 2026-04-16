using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 和了による局終了イベント
/// </summary>
/// <param name="WinnerIndices">和了者 (複数はダブロン/トリプルロン)</param>
/// <param name="LoserIndex">放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身 (= Winners[0])</param>
/// <param name="WinType">和了種別</param>
public record GameEventRoundEndedByWin(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType
) : GameEvent
{
    public override string Name => "局終了(和了)";
}
