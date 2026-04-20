using System.Collections.Immutable;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Scoring.Conversions;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Scoring.Shantens;
using GameHand = Mahjong.Lib.Game.Hands.Hand;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;
using TileKindList = Mahjong.Lib.Scoring.Tiles.TileKindList;

namespace Mahjong.Lib.Game.Scoring;

/// <summary>
/// Mahjong.Lib.Scoring をラップした IShantenEvaluator の本実装
/// </summary>
public sealed class ShantenEvaluatorImpl : IShantenEvaluator
{
    public int CalcShanten(GameHand hand, CallList callList)
    {
        var tileKindList = new TileKindList(hand.Select(x => TileKindConverter.FromKind(x.Kind)));
        return ShantenCalculator.Calc(tileKindList);
    }

    public ImmutableHashSet<int> EnumerateUsefulTileKinds(GameHand hand, CallList callList)
    {
        var tileKindList = new TileKindList(hand.Select(x => TileKindConverter.FromKind(x.Kind)));
        var current = ShantenCalculator.Calc(tileKindList);
        if (current == -1) { return []; }

        var builder = ImmutableHashSet.CreateBuilder<int>();
        foreach (var candidate in TileKind.All)
        {
            if (tileKindList.CountOf(candidate) >= 4) { continue; }
            var appended = tileKindList.Add(candidate);
            if (ShantenCalculator.Calc(appended) < current)
            {
                builder.Add(candidate.Value);
            }
        }
        return builder.ToImmutable();
    }
}
