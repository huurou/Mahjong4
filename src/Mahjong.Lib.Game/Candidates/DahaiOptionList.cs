using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 打牌候補の選択肢の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(DahaiOptionListBuilder), "Create")]
public record DahaiOptionList : IEnumerable<DahaiOption>
{
    private readonly ImmutableList<DahaiOption> options_;

    public DahaiOptionList() : this(Enumerable.Empty<DahaiOption>())
    {
    }

    public DahaiOptionList(IEnumerable<DahaiOption> options)
    {
        options_ = [.. options];
    }

    /// <summary>
    /// 選択肢の数
    /// </summary>
    public int Count => options_.Count;

    /// <summary>
    /// インデクサ
    /// </summary>
    public DahaiOption this[int index] => options_[index];

    public virtual bool Equals(DahaiOptionList? other)
    {
        return other is DahaiOptionList list && options_.SequenceEqual(list.options_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var option in options_)
        {
            hash.Add(option);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<DahaiOption> GetEnumerator()
    {
        return options_.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// DahaiOptionListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class DahaiOptionListBuilder
    {
        /// <summary>
        /// 指定された選択肢の配列から新しい選択肢リストを作成します
        /// </summary>
        public static DahaiOptionList Create(ReadOnlySpan<DahaiOption> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new DahaiOptionList(values.ToArray());
        }
    }
}
