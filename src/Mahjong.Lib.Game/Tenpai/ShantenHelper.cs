using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tenpai;

/// <summary>
/// シャンテン数計算と有効牌列挙のエントリポイント
/// Lib.Scoring の <see cref="ShantenCalculator"/> を直接呼び出す
/// </summary>
internal static class ShantenHelper
{
    public static int CalcShanten(Hands.Hand hand)
    {
        return ShantenCalculator.Calc(hand.ToScoringTileKindList());
    }

    /// <summary>
    /// 副露面子数を明示してシャンテン数を計算する。
    /// 自動推定 (14 - 手牌枚数) / 3 に頼らず、呼び出し側が保有する <see cref="Calls.CallList"/>
    /// から面子数を渡す想定。副露判定や副露シミュレーション経路で使用する。
    /// </summary>
    public static int CalcShanten(Hands.Hand hand, int knownCallMeldCount)
    {
        return ShantenCalculator.Calc(hand.ToScoringTileKindList(), knownCallMeldCount);
    }

    /// <summary>
    /// 指定手牌の有効牌 (引くとシャンテン数が減る牌種) 集合を返す。
    /// <paramref name="knownShanten"/> に呼び出し元で既に計算したシャンテン数を渡すと重複計算を避けられる
    /// </summary>
    public static ImmutableHashSet<TileKind> EnumerateUsefulTileKinds(Hands.Hand hand, int? knownShanten = null)
    {
        var tileKindList = hand.ToScoringTileKindList();
        var current = knownShanten ?? ShantenCalculator.Calc(tileKindList);
        if (current == -1)
        {
            return [];
        }

        var builder = ImmutableHashSet.CreateBuilder<TileKind>();
        foreach (var candidate in TileKind.All)
        {
            if (tileKindList.CountOf(candidate) >= 4) { continue; }

            var appended = tileKindList.Add(candidate);
            if (ShantenCalculator.Calc(appended) < current)
            {
                builder.Add(candidate);
            }
        }
        return builder.ToImmutable();
    }
}
