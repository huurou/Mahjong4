using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Rivers;

/// <summary>
/// 河
/// </summary>
public record River : TileList
{
    public River(IEnumerable<Tile> tiles) : base(tiles)
    {
    }
}
