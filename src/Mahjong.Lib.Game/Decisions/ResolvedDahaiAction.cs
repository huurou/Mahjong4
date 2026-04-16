using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 打牌採用結果
/// </summary>
/// <param name="Tile">打牌する牌</param>
/// <param name="IsRiichi">立直宣言か</param>
public record ResolvedDahaiAction(Tile Tile, bool IsRiichi = false) : ResolvedRoundAction;
