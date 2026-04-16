using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 流局による局終了イベント
/// </summary>
/// <param name="Type">流局種別</param>
/// <param name="TenpaiPlayers">テンパイ者 (荒牌平局時のみ意味を持つ、他は空配列)</param>
/// <param name="NagashiManganPlayers">流し満貫者 (荒牌平局時のみ意味を持つ、他は空配列)</param>
public record GameEventRoundEndedByRyuukyoku(
    RyuukyokuType Type,
    ImmutableArray<PlayerIndex> TenpaiPlayers,
    ImmutableArray<PlayerIndex> NagashiManganPlayers
) : GameEvent
{
    public override string Name => "局終了(流局)";
}
