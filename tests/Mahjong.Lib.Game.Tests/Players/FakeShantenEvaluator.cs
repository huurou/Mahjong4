using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Tenpai;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Players;

// テスト用の IShantenEvaluator スタブ
// CalcShanten / EnumerateUsefulTileKinds を任意のデリゲートで差し替える
internal sealed class FakeShantenEvaluator(
    Func<Hand, CallList, int>? calcShanten = null,
    Func<Hand, CallList, ImmutableHashSet<int>>? enumerateUseful = null
) : IShantenEvaluator
{
    public int CalcShanten(Hand hand, CallList callList)
    {
        return calcShanten?.Invoke(hand, callList) ?? 0;
    }

    public ImmutableHashSet<int> EnumerateUsefulTileKinds(Hand hand, CallList callList)
    {
        return enumerateUseful?.Invoke(hand, callList) ?? [];
    }
}
