using Mahjong.Lib.Scoring.Tiles;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Scoring.Calls;

/// <summary>
/// 副露の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(CallListBuilder), "Create")]
public record CallList() : IEnumerable<Call>, IComparable<CallList>
{
    /// <summary>
    /// 副露のコレクションを保持する不変リスト
    /// </summary>
    private readonly ImmutableList<Call> calls_ = [];

    /// <summary>
    /// 副露の数を取得します
    /// </summary>
    public int Count => calls_.Count;

    /// <summary>
    /// 門前でない副露が存在するかどうかを取得します
    /// 空の場合はfalse
    /// </summary>
    public bool HasOpen => calls_.Any(x => x.IsOpen);

    /// <summary>
    /// TileKindListのリストを取得します
    /// </summary>
    public ImmutableList<TileKindList> TileKindLists => [.. calls_.Select(x => x.TileKindList)];

    /// <summary>
    /// インデクサー
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>指定インデックスの副露</returns>
    public Call this[int index] => calls_[index];

    /// <summary>
    /// 指定された副露のコレクションから副露リストを作成します
    /// </summary>
    /// <param name="calls">副露のコレクション</param>
    public CallList(IEnumerable<Call> calls) : this()
    {
        var builder = ImmutableList.CreateBuilder<Call>();
        builder.AddRange(calls);
        builder.Sort();
        calls_ = builder.ToImmutable();
    }

    private CallList(ImmutableList<Call> immutableList) : this()
    {
        calls_ = immutableList.Sort();
    }

    /// <summary>
    /// 副露リストに新しい副露を追加します
    /// </summary>
    /// <param name="call">追加する副露</param>
    /// <returns>指定された副露が追加された新しい副露リスト</returns>
    public CallList Add(Call call)
    {
        return [.. calls_.Add(call)];
    }

    /// <summary>
    /// 副露リストに複数の副露を追加します
    /// </summary>
    /// <param name="calls">追加する副露のコレクション</param>
    /// <returns>指定された副露のコレクションが追加された新しい副露リスト</returns>
    public CallList AddRange(IEnumerable<Call> calls)
    {
        return [.. calls_.AddRange(calls)];
    }

    /// <summary>
    /// 副露リストから指定された副露を削除します
    /// </summary>
    /// <param name="call">削除対象の副露</param>
    /// <returns>指定された副露が削除された新しい副露リスト</returns>
    /// <exception cref="ArgumentException">指定された副露が存在しない場合</exception>
    public CallList Remove(Call call)
    {
        var newList = calls_.Remove(call);
        return newList.Count == calls_.Count
            ? throw new ArgumentException($"指定副露がありません。 call:{call}", nameof(call))
            : [.. newList];
    }

    /// <summary>
    /// 副露リストの列挙子を取得します
    /// </summary>
    /// <returns>副露リストの列挙子</returns>
    public IEnumerator<Call> GetEnumerator()
    {
        return calls_.GetEnumerator();
    }

    /// <summary>
    /// 副露リストの列挙子を取得します
    /// </summary>
    /// <returns>副露リストの列挙子</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 副露リストの文字列表現を取得します
    /// </summary>
    /// <returns>副露リストの文字列表現</returns>
    public override string ToString()
    {
        return string.Join(" ", calls_);
    }

    /// <summary>
    /// 指定したオブジェクトが現在のオブジェクトと等しいかどうかを判定します
    /// </summary>
    /// <param name="other">比較するオブジェクト</param>
    /// <returns>等しい場合はtrue、それ以外の場合はfalse</returns>
    public virtual bool Equals(CallList? other)
    {
        return other is not null && (ReferenceEquals(this, other) || calls_.SequenceEqual(other.calls_));
    }

    /// <summary>
    /// オブジェクトのハッシュコードを計算します
    /// </summary>
    /// <returns>このオブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        return calls_.Aggregate(0, (hash, call) => hash * 31 + call.GetHashCode());
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
    public int CompareTo(CallList? other)
    {
        if (other is null)
        {
            return 1;
        }

        var min = Math.Min(calls_.Count, other.calls_.Count);
        for (var i = 0; i < min; i++)
        {
            var comparison = calls_[i].CompareTo(other.calls_[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }
        return calls_.Count.CompareTo(other.calls_.Count);
    }

    /// <summary>
    /// 最初の CallList が二番目の CallList より小さいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の CallList</param>
    /// <param name="right">比較する二番目の CallList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより小さい場合は true、それ以外の場合は false</returns>
    public static bool operator <(CallList? left, CallList? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 最初の CallList が二番目の CallList より大きいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の CallList</param>
    /// <param name="right">比較する二番目の CallList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより大きい場合は true、それ以外の場合は false</returns>
    public static bool operator >(CallList? left, CallList? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 最初の CallList が二番目の CallList 以下かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の CallList</param>
    /// <param name="right">比較する二番目の CallList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以下の場合は true、それ以外の場合は false</returns>
    public static bool operator <=(CallList? left, CallList? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 最初の CallList が二番目の CallList 以上かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の CallList</param>
    /// <param name="right">比較する二番目の CallList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以上の場合は true、それ以外の場合は false</returns>
    public static bool operator >=(CallList? left, CallList? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// 副露リストを構築するためのビルダークラス
    /// </summary>
    public static class CallListBuilder
    {
        /// <summary>
        /// 指定された副露の配列から副露リストを作成します
        /// </summary>
        /// <param name="values">副露の配列</param>
        /// <returns>副露リスト</returns>
        public static CallList Create(ReadOnlySpan<Call> values)
        {
            // [.. ]を使用すると無限ループが発生する
            var builder = ImmutableList.CreateBuilder<Call>();
            foreach (var value in values) { builder.Add(value); }
            return new CallList(builder.ToImmutable());
        }
    }
}
