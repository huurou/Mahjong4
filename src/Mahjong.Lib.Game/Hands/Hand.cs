using Mahjong.Lib.Game.Tiles;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Hands;

/// <summary>
/// 晒していない手牌を表現するクラス
/// </summary>
public record Hand : IEnumerable<Tile>
{
    private readonly ImmutableList<Tile> tiles_;

    public Hand() : this(Enumerable.Empty<Tile>())
    {
    }

    public Hand(IEnumerable<Tile> tiles)
    {
        tiles_ = [.. tiles];
    }

    /// <summary>
    /// 指定の牌を追加した新しいHandを返す
    /// </summary>
    /// <param name="tile">追加する牌</param>
    /// <returns>牌を追加した新しいHand</returns>
    public Hand AddTile(Tile tile)
    {
        return new Hand(tiles_.Add(tile));
    }

    /// <summary>
    /// 指定の牌を1枚削除した新しいHandを返す
    /// </summary>
    /// <param name="tile">削除する牌</param>
    /// <returns>牌を削除した新しいHand</returns>
    public Hand RemoveTile(Tile tile)
    {
        if (!tiles_.Contains(tile))
        {
            throw new ArgumentException($"指定牌が手牌にありません。tile:{tile}", nameof(tile));
        }

        return new Hand(tiles_.Remove(tile));
    }

    public virtual bool Equals(Hand? other)
    {
        return other is Hand hand && tiles_.SequenceEqual(hand.tiles_);
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
