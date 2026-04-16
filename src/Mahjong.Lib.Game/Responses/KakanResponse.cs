using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 加槓応答
/// </summary>
/// <param name="Tile">加槓で追加する手牌の牌</param>
public record KakanResponse(Tile Tile) : AfterTsumoResponse;
