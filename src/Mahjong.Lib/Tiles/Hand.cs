using Mahjong.Lib.Calls;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Tiles;

/// <summary>
/// 晒していない手牌 TileKindListListのラッパー
/// </summary>
[CollectionBuilder(typeof(HandBuilder), "Create")]
public record Hand : TileKindListList
{
    /// <summary>
    /// 空の手牌を作成します
    /// </summary>
    public Hand() : base()
    {
    }

    /// <summary>
    /// 指定したTileKindListのコレクションから手牌を作成します
    /// </summary>
    /// <param name="tileKindLists">TileKindListのコレクション</param>
    public Hand(IEnumerable<TileKindList> tileKindLists) : base(tileKindLists)
    {
    }

    /// <summary>
    /// 手牌と副露を結合したTileKindListListを作成します
    /// </summary>
    /// <param name="callList">結合する副露リスト</param>
    /// <returns>手牌と副露を結合したTileKindListList</returns>
    public TileKindListList CombineFuuro(CallList callList)
    {
        return [.. this, .. callList.TileKindLists];
    }

    /// <summary>
    /// 和了牌を含むTileKindListのコレクションを取得します
    /// </summary>
    /// <param name="winTileKind">和了牌の種別</param>
    /// <returns>和了牌を含むTileKindListのリスト</returns>
    public ImmutableList<TileKindList> GetWinGroups(TileKind winTileKind)
    {
        return [.. this.Where(x => x.Contains(winTileKind)).Distinct()];
    }

    /// <summary>
    /// HandBuilderのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class HandBuilder
    {
        /// <summary>
        /// 指定したTileKindListの配列から新しいHandを作成します
        /// </summary>
        /// <param name="values">牌種別リストの配列</param>
        /// <returns>新しいHandオブジェクト</returns>
        public static Hand Create(ReadOnlySpan<TileKindList> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new(values.ToArray());
        }
    }
}
