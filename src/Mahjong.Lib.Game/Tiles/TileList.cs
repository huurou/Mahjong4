using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Game.Tiles;

/// <summary>
/// 牌の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(TileListBuilder), "Create")]
public record TileList : IEnumerable<Tile>
{
    private readonly ImmutableList<Tile> tiles_;

    public TileList(IEnumerable<Tile> tiles)
    {
        tiles_ = [.. tiles];
    }

    public virtual bool Equals(TileList? other)
    {
        return other is TileList tileList && tiles_.SequenceEqual(tileList.tiles_);
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
        return tiles_.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// TileListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class TileListBuilder
    {
        /// <summary>
        /// 指定された牌の配列から新しい牌リストを作成します
        /// </summary>
        /// <param name="values">牌リストに含める牌の配列</param>
        /// <returns>新しい牌リスト</returns>
        public static TileList Create(ReadOnlySpan<Tile> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new TileList(values.ToArray());
        }
    }
}
