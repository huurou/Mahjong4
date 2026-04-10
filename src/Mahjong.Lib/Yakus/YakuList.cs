using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Yakus;

/// <summary>
/// 役のコレクションを表現するクラス
/// </summary>
[CollectionBuilder(typeof(YakuListBuilder), "Create")]
public record YakuList() : IEnumerable<Yaku>, IEquatable<YakuList>
{
    /// <summary>
    /// 役リストの要素数を取得します
    /// </summary>
    public int Count => yakus_.Count;

    /// <summary>
    /// 副露時の合計翻数
    /// </summary>
    public int HanOpen => yakus_.Sum(x => x.HanOpen);

    /// <summary>
    /// 門前時の合計翻数
    /// </summary>
    public int HanClosed => yakus_.Sum(x => x.HanClosed);

    private readonly ImmutableList<Yaku> yakus_ = [];

    /// <summary>
    /// 指定された役のコレクションから新しい<see cref="YakuList"/>を作成します
    /// </summary>
    /// <param name="yakus">YakuListに含める役のコレクション</param>
    public YakuList(IEnumerable<Yaku> yakus) : this()
    {
        var builder = ImmutableList.CreateBuilder<Yaku>();
        builder.AddRange(yakus);
        builder.Sort();
        yakus_ = builder.ToImmutable();
    }

    private YakuList(ImmutableList<Yaku> immutableList) : this()
    {
        yakus_ = immutableList.Sort();
    }

    /// <summary>
    /// 指定された役を追加した新しい<see cref="YakuList"/>を返します
    /// </summary>
    /// <param name="yaku">追加する役</param>
    /// <returns>指定された役を含む新しい<see cref="YakuList"/></returns>
    public YakuList Add(Yaku yaku)
    {
        return [.. yakus_.Add(yaku)];
    }

    /// <summary>
    /// 指定された役のコレクションを追加した新しい<see cref="YakuList"/>を返します
    /// </summary>
    /// <param name="yakus">追加する役のコレクション</param>
    /// <returns>指定された役を全て含む新しい<see cref="YakuList"/></returns>
    public YakuList AddRange(IEnumerable<Yaku> yakus)
    {
        return [.. yakus_.AddRange(yakus)];
    }

    /// <summary>
    /// <see cref="YakuList"/>内の役を反復処理する列挙子を返します
    /// </summary>
    /// <returns>役の列挙子</returns>
    public IEnumerator<Yaku> GetEnumerator()
    {
        return yakus_.GetEnumerator();
    }

    /// <summary>
    /// <see cref="IEnumerable"/>インターフェースの実装として、役を反復処理する列挙子を返します
    /// </summary>
    /// <returns>役の列挙子</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 指定された<see cref="YakuList"/>オブジェクトとこのインスタンスが等しいかどうかを判断します
    /// </summary>
    /// <param name="other">比較対象の<see cref="YakuList"/>オブジェクト</param>
    /// <returns>等しい場合はtrue、それ以外の場合はfalse</returns>
    public virtual bool Equals(YakuList? other)
    {
        if (other is null) { return false; }
        if (ReferenceEquals(this, other)) { return true; }

        return yakus_.SequenceEqual(other.yakus_);
    }

    /// <summary>
    /// オブジェクトのハッシュコードを計算します
    /// </summary>
    /// <returns>このオブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        return yakus_.Aggregate(0, (hash, yaku) => hash * 31 + yaku.GetHashCode());
    }

    /// <summary>
    /// このYakuListの文字列表現を返します
    /// </summary>
    /// <returns>空白で区切られた役の文字列表現</returns>
    public sealed override string ToString()
    {
        return string.Join(" ", yakus_);
    }

    /// <summary>
    /// YakuListのビルダークラス
    /// </summary>
    public static class YakuListBuilder
    {
        /// <summary>
        /// 指定された役の配列から新しい<see cref="YakuList"/>を作成します
        /// </summary>
        /// <param name="values">YakuListに含める役の配列</param>
        /// <returns>指定された役を含む新しい<see cref="YakuList"/></returns>
        public static YakuList Create(ReadOnlySpan<Yaku> values)
        {
            // [.. ]を使用すると無限ループが発生する
            var builder = ImmutableList.CreateBuilder<Yaku>();
            foreach (var value in values) { builder.Add(value); }
            return new YakuList(builder.ToImmutable());
        }
    }
}
