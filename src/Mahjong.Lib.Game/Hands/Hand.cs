using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Hands;

/// <summary>
/// 晒していない手牌を表現するクラス
/// </summary>
public record Hand : TileList
{
    public Hand(IEnumerable<Tile> tiles) : base(tiles)
    {
    }
}
