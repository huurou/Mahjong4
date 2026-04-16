using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 嶺上ツモ後加槓応答
/// </summary>
/// <param name="Tile">加槓で追加する手牌の牌</param>
public record KanTsumoKakanResponse(Tile Tile) : AfterKanTsumoResponse;
