using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// チー応答
/// </summary>
/// <param name="HandTiles">手牌から使う2枚</param>
public record ChiResponse(ImmutableArray<Tile> HandTiles) : AfterDahaiResponse
{
    public virtual bool Equals(ChiResponse? other)
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
