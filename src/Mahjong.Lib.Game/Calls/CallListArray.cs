using Mahjong.Lib.Game.Players;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Calls;

/// <summary>
/// 各プレイヤーの副露リストの配列
/// </summary>
public record CallListArray : IEnumerable<CallList>
{
    private ImmutableArray<CallList> callLists_ = [.. Enumerable.Repeat(new CallList(), 4)];

    public CallList this[PlayerIndex index] => callLists_[index.Value];

    /// <summary>
    /// 指定のプレイヤーインデックスの副露リストに副露を追加した新しいCallListArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="call">追加する副露</param>
    /// <returns>副露を追加した新しいCallListArray</returns>
    public CallListArray AddCall(PlayerIndex index, Call call)
    {
        var builder = callLists_.ToBuilder();
        builder[index.Value] = builder[index.Value].Add(call);
        return new CallListArray { callLists_ = builder.ToImmutable() };
    }

    /// <summary>
    /// 指定のプレイヤーインデックスの副露リスト内で指定の副露を新しい副露に差し替えた新しいCallListArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="oldCall">差し替え対象の副露</param>
    /// <param name="newCall">新しい副露</param>
    /// <returns>差し替え後の新しいCallListArray</returns>
    public CallListArray ReplaceCall(PlayerIndex index, Call oldCall, Call newCall)
    {
        var builder = callLists_.ToBuilder();
        builder[index.Value] = builder[index.Value].Replace(oldCall, newCall);
        return new CallListArray { callLists_ = builder.ToImmutable() };
    }

    public IEnumerator<CallList> GetEnumerator()
    {
        return ((IEnumerable<CallList>)callLists_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)callLists_).GetEnumerator();
    }
}
