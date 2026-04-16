using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 打牌応答イベント
/// </summary>
/// <param name="Tile">打牌する牌</param>
/// <param name="IsRiichi">この打牌で立直宣言するか (合法性検証は Phase 5 の合法候補列挙で実装、Phase 2 では Round.Riichi 内で持ち点等の最低限のガードのみ)</param>
public record RoundEventResponseDahai(Tile Tile, bool IsRiichi = false) : RoundEvent
{
    public override string Name => "打牌応答";
}
