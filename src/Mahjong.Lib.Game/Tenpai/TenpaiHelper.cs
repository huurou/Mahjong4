using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Scoring.HandCalculating.HandDividing;
using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tenpai;

/// <summary>
/// テンパイ判定と待ち牌種列挙のエントリポイント
/// Lib.Scoring の <see cref="ShantenCalculator"/> / <see cref="HandDivider"/> を直接呼び出す
/// </summary>
internal static class TenpaiHelper
{
    public static bool IsTenpai(Hands.Hand hand)
    {
        return ShantenCalculator.Calc(hand.ToScoringTileKindList()) == 0;
    }

    public static ImmutableHashSet<TileKind> EnumerateWaitTileKinds(Hands.Hand hand)
    {
        var tileKindList = hand.ToScoringTileKindList();
        if (ShantenCalculator.Calc(tileKindList) != 0)
        {
            return [];
        }

        var builder = ImmutableHashSet.CreateBuilder<TileKind>();
        foreach (var candidate in TileKind.All)
        {
            if (tileKindList.CountOf(candidate) >= 4) { continue; }

            var appended = tileKindList.Add(candidate);
            if (ShantenCalculator.Calc(appended) == -1)
            {
                builder.Add(candidate);
            }
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// 指定の牌種 <paramref name="kind"/> が <paramref name="hand"/> のすべてのテンパイ解釈において
    /// 刻子 (暗刻) として使われるかを判定します。順子として使う分解解釈が 1 つでも存在する場合は <c>false</c>。
    /// 立直中の暗槓送り槓禁止判定で使用します
    /// </summary>
    public static bool IsKoutsuOnlyInAllInterpretations(Hands.Hand hand, TileKind kind)
    {
        var waits = EnumerateWaitTileKinds(hand);
        if (waits.Count == 0) { return false; }

        var tileKindList = hand.ToScoringTileKindList();

        foreach (var wait in waits)
        {
            var completed = tileKindList.Add(wait);
            var divisions = HandDivider.Divide(completed);
            if (divisions.Count == 0) { continue; }

            foreach (var division in divisions)
            {
                foreach (var mentsu in division)
                {
                    if (mentsu.IsShuntsu && mentsu.Any(x => x == kind))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
