using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 応答候補の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(CandidateListBuilder), "Create")]
public record CandidateList : IEnumerable<ResponseCandidate>
{
    private readonly ImmutableList<ResponseCandidate> candidates_;

    public CandidateList() : this(Enumerable.Empty<ResponseCandidate>())
    {
    }

    public CandidateList(IEnumerable<ResponseCandidate> candidates)
    {
        candidates_ = [.. candidates];
    }

    /// <summary>
    /// 候補の数
    /// </summary>
    public int Count => candidates_.Count;

    /// <summary>
    /// インデクサ
    /// </summary>
    public ResponseCandidate this[int index] => candidates_[index];

    /// <summary>
    /// 指定の型の候補が含まれているかを返す
    /// </summary>
    public bool HasCandidate<T>() where T : ResponseCandidate
    {
        return candidates_.Any(x => x is T);
    }

    /// <summary>
    /// 指定の型の候補を列挙する
    /// </summary>
    public IEnumerable<T> GetCandidates<T>() where T : ResponseCandidate
    {
        return candidates_.OfType<T>();
    }

    public virtual bool Equals(CandidateList? other)
    {
        return other is CandidateList list && candidates_.SequenceEqual(list.candidates_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var candidate in candidates_)
        {
            hash.Add(candidate);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<ResponseCandidate> GetEnumerator()
    {
        return candidates_.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// CandidateListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class CandidateListBuilder
    {
        /// <summary>
        /// 指定された候補の配列から新しい候補リストを作成します
        /// </summary>
        public static CandidateList Create(ReadOnlySpan<ResponseCandidate> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new CandidateList(values.ToArray());
        }
    }
}
