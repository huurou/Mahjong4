using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Fus;

/// <summary>
/// 符の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(FuListBuilder), "Create")]
public record FuList() : IEnumerable<Fu>
{
    /// <summary>
    /// 符の数を取得します
    /// </summary>
    public int Count => fus_.Count;

    /// <summary>
    /// 符の合計値を取得します
    /// 七対子の場合は25符固定、それ以外は10の単位に切り上げます
    /// </summary>
    public int Total =>
        Count == 0 ? 0
        : this.Any(x => x.Type == FuType.Chiitoitsu) ? 25
        : (this.Sum(x => x.Value) + 9) / 10 * 10; // 10の単位に切り上げる

    private readonly ImmutableList<Fu> fus_ = [];

    /// <summary>
    /// 指定された符のコレクションから新しい符リストを初期化します
    /// </summary>
    /// <param name="fus">符のコレクション</param>
    public FuList(IEnumerable<Fu> fus) : this()
    {
        var builder = ImmutableList.CreateBuilder<Fu>();
        builder.AddRange(fus);
        builder.Sort();
        fus_ = builder.ToImmutable();
    }

    private FuList(ImmutableList<Fu> immutableList) : this()
    {
        fus_ = immutableList.Sort();
    }

    public FuList Add(Fu fu)
    {
        return [.. fus_.Add(fu)];
    }

    /// <summary>
    /// 符リストの文字列表現を取得します
    /// 「合計符数 符1,符2,...」の形式で表示します
    /// </summary>
    /// <returns>符リストの文字列表現</returns>
    public sealed override string ToString()
    {
        return $"{Total}符 {string.Join(',', fus_)}";
    }

    public IEnumerator<Fu> GetEnumerator()
    {
        return fus_.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 指定されたFuListオブジェクトと現在のFuListオブジェクトが等しいかどうかを判断します
    /// </summary>
    /// <param name="other">比較対象のFuListオブジェクト</param>
    /// <returns>指定されたFuListが現在のFuListと等しい場合はtrue、それ以外の場合はfalse</returns>
    public virtual bool Equals(FuList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.SequenceEqual(other);
    }

    /// <summary>
    /// オブジェクトのハッシュコードを計算します
    /// </summary>
    /// <returns>このオブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        return fus_.Aggregate(0, (hash, fu) => hash * 31 + fu.GetHashCode());
    }

    public static class FuListBuilder
    {
        public static FuList Create(ReadOnlySpan<Fu> values)
        {
            // [.. ]を使用すると無限ループが発生する
            var builder = ImmutableList.CreateBuilder<Fu>();
            foreach (var value in values) { builder.Add(value); }
            return new FuList(builder.ToImmutable());
        }
    }
}
