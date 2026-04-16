using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 大明槓応答候補 手牌から使う3枚を提示する
/// </summary>
/// <param name="HandTiles">手牌から使う3枚</param>
public record DaiminkanCandidate(ImmutableArray<Tile> HandTiles) : ResponseCandidate
{
    public virtual bool Equals(DaiminkanCandidate? other)
    {
        return other is not null && HandTiles.SequenceEqual(other.HandTiles);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var tile in HandTiles)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }
}
