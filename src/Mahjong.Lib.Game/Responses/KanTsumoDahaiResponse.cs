using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 嶺上ツモ後打牌応答
/// </summary>
/// <param name="Tile">打牌する牌</param>
/// <param name="IsRiichi">この打牌で立直宣言するか</param>
public record KanTsumoDahaiResponse(Tile Tile, bool IsRiichi = false) : AfterKanTsumoResponse;
