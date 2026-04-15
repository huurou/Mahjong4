using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rivers;

/// <summary>
/// 各プレイヤーの河の配列
/// </summary>
public record RiverArray : IEnumerable<River>
{
    private ImmutableArray<River> rivers_ = [.. Enumerable.Repeat(new River(), 4)];

    public River this[PlayerIndex index] => rivers_[index.Value];

    /// <summary>
    /// 指定のプレイヤーインデックスの河に牌を追加した新しいRiverArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="tile">追加する牌</param>
    /// <returns>牌を追加した新しいRiverArray</returns>
    public RiverArray AddTile(PlayerIndex index, Tile tile)
    {
        var builder = rivers_.ToBuilder();
        builder[index.Value] = builder[index.Value].AddTile(tile);
        return new RiverArray { rivers_ = builder.ToImmutable() };
    }

    /// <summary>
    /// 指定のプレイヤーインデックスの河から最後の牌を削除した新しいRiverArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="tile">削除された牌 河が空の場合はnull</param>
    /// <returns>最後の牌を削除した新しいRiverArray</returns>
    public RiverArray RemoveLastTile(PlayerIndex index, out Tile? tile)
    {
        var builder = rivers_.ToBuilder();
        var newRiver = builder[index.Value].RemoveLastTile(out tile);
        if (tile is not null)
        {
            builder[index.Value] = newRiver;
            return new RiverArray { rivers_ = builder.ToImmutable() };
        }
        else
        {
            return this;
        }
    }

    public virtual bool Equals(RiverArray? other)
    {
        return other is RiverArray array && rivers_.SequenceEqual(array.rivers_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var river in rivers_)
        {
            hash.Add(river);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<River> GetEnumerator()
    {
        return ((IEnumerable<River>)rivers_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)rivers_).GetEnumerator();
    }
}
