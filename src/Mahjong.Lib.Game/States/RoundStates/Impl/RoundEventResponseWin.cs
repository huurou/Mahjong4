using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 和了応答イベント
/// </summary>
/// <param name="WinnerIndices">和了者 (ダブロン/トリロンなら複数)</param>
/// <param name="LoserIndex">放銃者。ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身 (= Winners[0]) をそのまま入れる</param>
/// <param name="WinType">和了種別</param>
public record RoundEventResponseWin(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType
) : RoundEvent
{
    public override string Name => "和了応答";
}
