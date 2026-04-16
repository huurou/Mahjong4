using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 加槓応答候補 加槓可能な手牌の牌を提示する
/// </summary>
/// <param name="Tile">加槓で追加する手牌の牌</param>
public record KakanCandidate(Tile Tile) : ResponseCandidate;
