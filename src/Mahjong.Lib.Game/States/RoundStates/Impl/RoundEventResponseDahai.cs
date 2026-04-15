using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 打牌応答イベント
/// </summary>
/// <param name="Tile">打牌する牌</param>
public record RoundEventResponseDahai(Tile Tile) : RoundEvent
{
    public override string Name => "打牌応答";
}
