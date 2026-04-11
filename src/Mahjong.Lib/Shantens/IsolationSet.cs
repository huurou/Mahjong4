using Mahjong.Lib.Tiles;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Shantens;

/// <summary>
/// 孤立牌の集合を表すクラスです。
/// 雀頭や面子、塔子などに含まれない牌を管理します。
/// </summary>
[CollectionBuilder(typeof(IsolationSetBuilder), "Create")]
internal record IsolationSet() : IEnumerable<TileKind>
{
    private readonly ImmutableHashSet<TileKind> tileKindSet_ = [];

    /// <summary>
    /// 要素数を取得します。
    /// </summary>
    public int Count => tileKindSet_.Count;

    /// <summary>
    /// 指定された牌のコレクションから新しい孤立牌セットを作成します。
    /// </summary>
    /// <param name="tiles">初期化に使用する牌のコレクション</param>
    public IsolationSet(IEnumerable<TileKind> tiles) : this()
    {
        tileKindSet_ = [.. tiles];
    }

    /// <summary>
    /// 孤立牌セットの列挙子を取得します。
    /// </summary>
    /// <returns>孤立牌セットの列挙子</returns>
    public IEnumerator<TileKind> GetEnumerator()
    {
        return tileKindSet_.GetEnumerator();
    }

    /// <summary>
    /// 非ジェネリックコレクションの列挙子を取得します。
    /// </summary>
    /// <returns>非ジェネリックコレクションの列挙子</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// IsolationSetのコレクションビルダーを提供するクラスです。
    /// </summary>
    internal static class IsolationSetBuilder
    {
        /// <summary>
        /// ReadOnlySpanからIsolationSetを作成します。
        /// </summary>
        /// <param name="values">IsolationSetに含める牌のReadOnlySpan</param>
        /// <returns>作成されたIsolationSet</returns>
        public static IsolationSet Create(ReadOnlySpan<TileKind> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new(values.ToArray());
        }
    }
}
