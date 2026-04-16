using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 嶺上ツモ後暗槓応答
/// </summary>
/// <param name="Tile">暗槓する牌種の牌</param>
public record KanTsumoAnkanResponse(Tile Tile) : AfterKanTsumoResponse;
