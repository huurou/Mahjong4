using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 暗槓応答候補 暗槓対象の4枚を提示する
/// </summary>
/// <param name="Tiles">暗槓対象の4枚</param>
public record AnkanCandidate(ImmutableArray<Tile> Tiles) : ResponseCandidate
{
    public virtual bool Equals(AnkanCandidate? other)
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
