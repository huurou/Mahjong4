using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 暗槓採用結果
/// </summary>
public record ResolvedAnkanAction : ResolvedKanAction
{
    /// <summary>暗槓対象の4枚</summary>
    public ImmutableArray<Tile> Tiles { get; init; }

    public ResolvedAnkanAction(ImmutableArray<Tile> tiles)
    {
        if (tiles.Length != 4)
        {
            throw new ArgumentException($"暗槓では4枚の牌が必要です。実際:{tiles.Length}枚", nameof(tiles));
        }

        Tiles = tiles;
    }

    public virtual bool Equals(ResolvedAnkanAction? other)
    {
        return other is not null && Tiles.SequenceEqual(other.Tiles);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var tile in Tiles)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }
}
