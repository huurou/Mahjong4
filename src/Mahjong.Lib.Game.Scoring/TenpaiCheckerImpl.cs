using System.Collections.Immutable;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Scoring.Conversions;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Scoring.HandCalculating.HandDividing;
using Mahjong.Lib.Scoring.Shantens;
using GameHand = Mahjong.Lib.Game.Hands.Hand;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;
using TileKindList = Mahjong.Lib.Scoring.Tiles.TileKindList;

namespace Mahjong.Lib.Game.Scoring;

/// <summary>
/// Mahjong.Lib.Scoring をラップした ITenpaiChecker の本実装
/// </summary>
public sealed class TenpaiCheckerImpl : ITenpaiChecker
{
    public bool IsTenpai(GameHand hand, CallList callList)
    {
        var tileKindList = new TileKindList(hand.Select(x => TileKindConverter.FromKind(x.Kind)));
        return ShantenCalculator.Calc(tileKindList) == 0;
    }

    public ImmutableHashSet<int> EnumerateWaitTileKinds(GameHand hand, CallList callList)
    {
        var tileKindList = new TileKindList(hand.Select(x => TileKindConverter.FromKind(x.Kind)));
        if (ShantenCalculator.Calc(tileKindList) != 0)
        {
            return [];
        }

        var builder = ImmutableHashSet.CreateBuilder<int>();
        foreach (var candidate in TileKind.All)
        {
            if (tileKindList.CountOf(candidate) >= 4) { continue; }
            var appended = tileKindList.Add(candidate);
            if (ShantenCalculator.Calc(appended) == -1)
            {
                builder.Add(candidate.Value);
            }
        }
        return builder.ToImmutable();
    }

    public bool IsKoutsuOnlyInAllInterpretations(GameHand hand, CallList callList, int kind)
    {
        var waits = EnumerateWaitTileKinds(hand, callList);
        if (waits.Count == 0) { return false; }

        var tileKindList = new TileKindList(hand.Select(x => TileKindConverter.FromKind(x.Kind)));
        var targetKind = TileKindConverter.FromKind(kind);

        foreach (var waitValue in waits)
        {
            var completed = tileKindList.Add(TileKindConverter.FromKind(waitValue));
            var divisions = HandDivider.Divide(completed);
            if (divisions.Count == 0) { continue; }

            foreach (var division in divisions)
            {
                foreach (var mentsu in division)
                {
                    if (mentsu.IsShuntsu && mentsu.Any(x => x == targetKind))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
