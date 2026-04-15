using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Hands;

/// <summary>
/// 各プレイヤーの手牌の配列
/// </summary>
public record HandArray : IEnumerable<Hand>
{
    private ImmutableArray<Hand> hands_ = [.. Enumerable.Repeat(new Hand(), 4)];

    public Hand this[PlayerIndex index] => hands_[index.Value];

    /// <summary>
    /// 指定のプレイヤーインデックスの手牌に牌を追加した新しいHandArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="tile">追加する牌</param>
    /// <returns>牌を追加した新しいHandArray</returns>
    public HandArray AddTile(PlayerIndex index, Tile tile)
    {
        var builder = hands_.ToBuilder();
        builder[index.Value] = builder[index.Value].AddTile(tile);
        return new HandArray { hands_ = builder.ToImmutable() };
    }

    /// <summary>
    /// 指定のプレイヤーインデックスの手牌に牌を複数枚まとめて追加した新しいHandArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="tiles">追加する牌の列</param>
    /// <returns>牌を追加した新しいHandArray</returns>
    public HandArray AddTiles(PlayerIndex index, IEnumerable<Tile> tiles)
    {
        var builder = hands_.ToBuilder();
        var hand = builder[index.Value];
        foreach (var tile in tiles)
        {
            hand = hand.AddTile(tile);
        }
        builder[index.Value] = hand;
        return new HandArray { hands_ = builder.ToImmutable() };
    }

    /// <summary>
    /// 指定のプレイヤーインデックスの手牌から牌を1枚削除した新しいHandArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="tile">削除する牌</param>
    /// <returns>牌を削除した新しいHandArray</returns>
    public HandArray RemoveTile(PlayerIndex index, Tile tile)
    {
        var builder = hands_.ToBuilder();
        builder[index.Value] = builder[index.Value].RemoveTile(tile);
        return new HandArray { hands_ = builder.ToImmutable() };
    }

    public virtual bool Equals(HandArray? other)
    {
        return other is HandArray array && hands_.SequenceEqual(array.hands_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var hand in hands_)
        {
            hash.Add(hand);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<Hand> GetEnumerator()
    {
        return ((IEnumerable<Hand>)hands_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)hands_).GetEnumerator();
    }
}
