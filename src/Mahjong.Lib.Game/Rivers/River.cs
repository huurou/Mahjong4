using Mahjong.Lib.Game.Tiles;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rivers;

/// <summary>
/// 河
/// </summary>
public record River : IEnumerable<Tile>
{
    private readonly ImmutableList<Tile> tiles_;

    public River() : this(Enumerable.Empty<Tile>())
    {
    }

    public River(IEnumerable<Tile> tiles)
    {
        tiles_ = [.. tiles];
    }

    public River AddTile(Tile tile)
    {
        return new River(tiles_.Add(tile));
    }

    public River RemoveLastTile(out Tile? tile)
    {
        if (tiles_.Count != 0)
        {
            tile = tiles_[^1];
            return new River(tiles_.RemoveAt(tiles_.Count - 1));
        }
        else
        {
            tile = null;
            return this;
        }
    }

    public virtual bool Equals(River? other)
    {
        return other is River river && tiles_.SequenceEqual(river.tiles_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var tile in tiles_)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<Tile> GetEnumerator()
    {
        return ((IEnumerable<Tile>)tiles_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)tiles_).GetEnumerator();
    }
}
