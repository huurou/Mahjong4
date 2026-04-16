using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 流局応答イベント
/// </summary>
/// <param name="Type">流局種別</param>
/// <param name="TenpaiPlayers">テンパイ者 (荒牌平局時のみ意味を持つ、他は空配列)</param>
public record RoundEventResponseRyuukyoku(
    RyuukyokuType Type,
    ImmutableArray<PlayerIndex> TenpaiPlayers
) : RoundEvent
{
    public override string Name => "流局応答";
}
