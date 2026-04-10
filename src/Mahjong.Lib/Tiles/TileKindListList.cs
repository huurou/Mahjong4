using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Tiles;

/// <summary>
/// 牌種別の集合の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(TileKindListListBuilder), "Create")]
public record TileKindListList() : IEnumerable<TileKindList>, IComparable<TileKindListList>
{
    private readonly ImmutableList<TileKindList> tileLists_ = [];

    /// <summary>
    /// 要素数を取得します
    /// </summary>
    public int Count => tileLists_.Count;

    /// <summary>
    /// 指定したインデックスのTileKindListを取得します
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>指定したインデックスのTileKindList</returns>
    public TileKindList this[int index] => tileLists_[index];

    /// <summary>
    /// 指定した牌牌種別リストのコレクションから新しいTileKindListListを作成します
    /// </summary>
    /// <param name="tileKindLists">牌種別リストのコレクション</param>
    public TileKindListList(IEnumerable<TileKindList> tileKindLists) : this()
    {
        var builder = ImmutableList.CreateBuilder<TileKindList>();
        builder.AddRange(tileKindLists);
        tileLists_ = builder.ToImmutable();
    }

    private TileKindListList(ImmutableList<TileKindList> immutableList) : this()
    {
        tileLists_ = immutableList;
    }

    /// <summary>
    /// 指定した牌種別の刻子か槓子が含まれているかどうかを判定します
    /// </summary>
    /// <param name="tileKind">検索する牌種別</param>
    /// <returns>刻子または槓子が含まれている場合はtrue、それ以外の場合はfalse</returns>
    public bool IncludesKoutsu(TileKind tileKind)
    {
        return this.Any(x => (x.IsKoutsu || x.IsKantsu) && x[0] == tileKind);
    }

    /// <summary>
    /// 指定した牌種別リストを追加した新しいTileKindListListを返します
    /// </summary>
    /// <param name="tileKindList">追加する牌種別リスト</param>
    /// <returns>牌種別リストが追加された新しいTileKindListList</returns>
    public TileKindListList Add(TileKindList tileKindList)
    {
        return new TileKindListList(tileLists_.Add(tileKindList));
    }

    /// <summary>
    /// 指定した牌種別リストのコレクションを追加した新しいTileKindListListを返します
    /// </summary>
    /// <param name="tileKindLists">追加する牌種別リストのコレクション</param>
    /// <returns>牌種別リストのコレクションが追加された新しいTileKindListList</returns>
    public TileKindListList AddRange(IEnumerable<TileKindList> tileKindLists)
    {
        return new TileKindListList(tileLists_.AddRange(tileKindLists));
    }

    /// <summary>
    /// 指定した牌種別リストを削除した新しいTileKindListListを返します
    /// </summary>
    /// <param name="tileKindList">削除する牌種別リスト</param>
    /// <returns>牌種別リストが削除された新しいTileKindListList</returns>
    public TileKindListList Remove(TileKindList tileKindList)
    {
        var newList = tileLists_.Remove(tileKindList);
        if (newList.Count == tileLists_.Count)
        {
            throw new ArgumentException($"指定リストがありません。 tileKindList:{tileKindList}", nameof(tileKindList));
        }
        return new TileKindListList(newList);
    }

    /// <summary>
    /// 列挙子を返します
    /// </summary>
    /// <returns>コレクションを反復処理する列挙子</returns>
    public IEnumerator<TileKindList> GetEnumerator()
    {
        return tileLists_.GetEnumerator();
    }

    /// <summary>
    /// 列挙子を返します
    /// </summary>
    /// <returns>コレクションを反復処理する列挙子</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 指定したオブジェクトが現在のオブジェクトと等しいかどうかを判定します
    /// </summary>
    /// <param name="other">比較するオブジェクト</param>
    /// <returns>等しい場合はtrue、それ以外の場合はfalse</returns>
    public virtual bool Equals(TileKindListList? other)
    {
        return other is not null && this.OrderBy(x => x).SequenceEqual(other.OrderBy(x => x));
    }

    /// <summary>
    /// オブジェクトのハッシュコードを計算します
    /// </summary>
    /// <returns>このオブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        return this.OrderBy(x => x).Aggregate(0, (hash, tileList) => hash * 31 + tileList.GetHashCode());
    }

    /// <summary>
    /// 現在のインスタンスを指定したオブジェクトと比較し、並び順での相対位置を示す整数を返します
    /// </summary>
    /// <param name="other">比較するオブジェクト</param>
    /// <returns>
    /// 現在のインスタンスが比較対象オブジェクトより前にある場合は負の値、
    /// 同じ位置にある場合は 0、
    /// 後にある場合は正の値
    /// </returns>
    public int CompareTo(TileKindListList? other)
    {
        if (other is null)
        {
            return 1;
        }

        var sortedThis = this.OrderBy(x => x).ToList();
        var sortedOther = other.OrderBy(x => x).ToList();
        var min = Math.Min(sortedThis.Count, sortedOther.Count);
        for (var i = 0; i < min; i++)
        {
            var comparison = sortedThis[i].CompareTo(sortedOther[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }
        return sortedThis.Count.CompareTo(sortedOther.Count);
    }

    /// <summary>
    /// オブジェクトの文字列表現を返します
    /// </summary>
    /// <returns>オブジェクトの文字列表現</returns>
    public sealed override string ToString()
    {
        return string.Join("", this.Select(x => $"[{x}]"));
    }

    /// <summary>
    /// 最初の TileKindListList が二番目の TileKindListList より小さいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindListList</param>
    /// <param name="right">比較する二番目の TileKindListList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより小さい場合は true、それ以外の場合は false</returns>
    public static bool operator <(TileKindListList? left, TileKindListList? right)
    {
        if (left is null) return right is not null;
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 最初の TileKindListList が二番目の TileKindListList より大きいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindListList</param>
    /// <param name="right">比較する二番目の TileKindListList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより大きい場合は true、それ以外の場合は false</returns>
    public static bool operator >(TileKindListList? left, TileKindListList? right)
    {
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 最初の TileKindListList が二番目の TileKindListList 以下かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindListList</param>
    /// <param name="right">比較する二番目の TileKindListList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以下の場合は true、それ以外の場合は false</returns>
    public static bool operator <=(TileKindListList? left, TileKindListList? right)
    {
        if (left is null) return true;
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 最初の TileKindListList が二番目の TileKindListList 以上かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindListList</param>
    /// <param name="right">比較する二番目の TileKindListList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以上の場合は true、それ以外の場合は false</returns>
    public static bool operator >=(TileKindListList? left, TileKindListList? right)
    {
        if (left is null) return right is null;
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// TileKindListListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class TileKindListListBuilder
    {
        /// <summary>
        /// 指定した牌種別リストの配列から新しいTileKindListListを作成します
        /// </summary>
        /// <param name="values">牌種別リストの配列</param>
        /// <returns>新しいTileKindListListインスタンス</returns>
        public static TileKindListList Create(ReadOnlySpan<TileKindList> values)
        {
            var builder = ImmutableList.CreateBuilder<TileKindList>();
            foreach (var value in values) { builder.Add(value); }
            return new TileKindListList(builder.ToImmutable());
        }
    }
}
