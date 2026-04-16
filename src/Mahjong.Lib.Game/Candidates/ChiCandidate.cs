using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// チー応答候補 手牌から使う2枚の組み合わせを提示する
/// 複数の組み合わせがある場合は複数の ChiCandidate で表現する
/// </summary>
/// <param name="HandTiles">手牌から使う2枚</param>
public record ChiCandidate(ImmutableArray<Tile> HandTiles) : ResponseCandidate
{
    public virtual bool Equals(ChiCandidate? other)
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
