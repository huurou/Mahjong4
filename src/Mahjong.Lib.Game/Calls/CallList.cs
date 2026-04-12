using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Game.Calls;

/// <summary>
/// 副露の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(CallListBuilder), "Create")]
public record CallList : IEnumerable<Call>
{
    private readonly ImmutableList<Call> calls_;
    public CallList(IEnumerable<Call> calls)
    {
        calls_ = [.. calls];
    }

    public virtual bool Equals(CallList? other)
    {
        return other is CallList callList && calls_.SequenceEqual(callList.calls_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var call in calls_)
        {
            hash.Add(call);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<Call> GetEnumerator()
    {
        return calls_.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// CallListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class CallListBuilder
    {
        /// <summary>
        /// 指定された副露の配列から新しい副露リストを作成します
        /// </summary>
        /// <param name="values">副露リストに含める副露の配列</param>
        /// <returns>新しい副露リスト</returns>
        public static CallList Create(ReadOnlySpan<Call> values)
        {
            // [.. ]を使用すると無限ループが発生する
            return new CallList(values.ToArray());
        }
    }
}
