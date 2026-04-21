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

    public static ImmutableHashSet<TileKind> EnumerateUsefulTileKinds(Hands.Hand hand)
    {
        var tileKindList = hand.ToScoringTileKindList();
        var current = ShantenCalculator.Calc(tileKindList);
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
