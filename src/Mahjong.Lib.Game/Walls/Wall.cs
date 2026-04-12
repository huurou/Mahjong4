using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Walls;

/// <summary>
/// 山牌を表現するクラス
/// </summary>
public record Wall
{
    private readonly TileList tiles_;

    public Wall(IEnumerable<Tile> tiles)
    {
        tiles_ = new(tiles);
    }

    public virtual bool Equals(Wall? other)
    {
        return other is Wall wall && tiles_.SequenceEqual(wall.tiles_);
    }

    public override int GetHashCode()
    {
        return tiles_.GetHashCode();
    }
}
