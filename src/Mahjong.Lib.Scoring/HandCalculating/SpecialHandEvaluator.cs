using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;
using Mahjong.Lib.Scoring.Yakus.Impl;
using System.Diagnostics.CodeAnalysis;

namespace Mahjong.Lib.Scoring.HandCalculating;

/// <summary>
/// 特殊役（流し満貫、国士無双、天和）の判定を行います
/// </summary>
internal static class SpecialHandEvaluator
{
    /// <summary>
    /// 流し満貫の判定（winTile==nullはHandValidatorで検証済み前提）
    /// </summary>
    internal static bool EvaluateNagashimangan(WinSituation winSituation, TileKind? winTile, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (Nagashimangan.Valid(winSituation))
        {
            handResult = HandResult.Create([Yaku.Nagashimangan], winSituation: winSituation);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 国士無双の判定 国士無双は適切なHandに分解されないので先に処理する
    /// </summary>
    internal static bool EvaluateKokushimusou(TileKindList tileKindList, TileKind? winTile, WinSituation winSituation, GameRules gameRules, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (!Kokushimusou.Valid(tileKindList))
        {
            return false;
        }

        var yakuList = BuildKokushimusouYakuList(tileKindList, winTile, gameRules);
        yakuList = AddSpecialWinYaku(yakuList, winSituation, gameRules);

        handResult = HandResult.Create(yakuList, winSituation: winSituation, gameRules: gameRules);
        return true;
    }

    /// <summary>
    /// 天和判定を行う 国士無双は除く
    /// </summary>
    internal static YakuList EvaluateTenhou(Hand hand, WinSituation winSituation, GameRules gameRules)
    {
        var yakuList = new YakuList();

        if (Tenhou.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Tenhou);
        }

        yakuList = AddTenhouSpecificYaku(yakuList, hand, gameRules);

        return yakuList;
    }

    /// <summary>
    /// 国士無双の役リストを構築します
    /// </summary>
    private static YakuList BuildKokushimusouYakuList(TileKindList tileKindList, TileKind? winTile, GameRules gameRules)
    {
        var yakuList = new YakuList();

        if (winTile is not null && Kokushimusou13menmachi.Valid(tileKindList, winTile))
        {
            if (Kokushimusou13menmachiDouble.Valid(tileKindList, winTile, gameRules))
            {
                yakuList = yakuList.Add(Yaku.Kokushimusou13menmachiDouble);
            }
            else
            {
                yakuList = yakuList.Add(Yaku.Kokushimusou13menmachi);
            }
        }
        else
        {
            yakuList = yakuList.Add(Yaku.Kokushimusou);
        }

        return yakuList;
    }

    /// <summary>
    /// 特殊和了役（天和、地和、人和役満）を追加します
    /// </summary>
    private static YakuList AddSpecialWinYaku(YakuList yakuList, WinSituation winSituation, GameRules gameRules)
    {
        if (Tenhou.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Tenhou);
        }

        if (Chiihou.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Chiihou);
        }

        if (RenhouYakuman.Valid(winSituation, gameRules))
        {
            yakuList = yakuList.Add(Yaku.RenhouYakuman);
        }

        return yakuList;
    }

    /// <summary>
    /// 天和時の特殊役を追加します
    /// </summary>
    private static YakuList AddTenhouSpecificYaku(YakuList yakuList, Hand hand, GameRules gameRules)
    {
        if (Chuurenpoutou.Valid(hand))
        {
            yakuList = yakuList.Add(Yaku.Chuurenpoutou);
        }

        // ツモ牌がないので特別に四暗刻の判定を行う
        if (hand.Count(x => x.IsKoutsu) == 4)
        {
            yakuList = yakuList.Add(Yaku.Suuankou);
        }

        yakuList = AddBasicYakumanForTenhou(yakuList, hand, gameRules);

        return yakuList;
    }

    /// <summary>
    /// 天和時の基本的な役満を追加します
    /// </summary>
    private static YakuList AddBasicYakumanForTenhou(YakuList yakuList, Hand hand, GameRules gameRules)
    {
        if (Daisangen.Valid(hand, []))
        {
            yakuList = yakuList.Add(Yaku.Daisangen);
        }

        if (Shousuushii.Valid(hand, []))
        {
            yakuList = yakuList.Add(Yaku.Shousuushii);
        }

        if (Daisuushii.Valid(hand, []))
        {
            if (gameRules.DoubleYakumanEnabled)
            {
                yakuList = yakuList.Add(Yaku.DaisuushiiDouble);
            }
            else
            {
                yakuList = yakuList.Add(Yaku.Daisuushii);
            }
        }

        if (Ryuuiisou.Valid(hand, []))
        {
            yakuList = yakuList.Add(Yaku.Ryuuiisou);
        }

        if (Tsuuiisou.Valid(hand, []))
        {
            yakuList = yakuList.Add(Yaku.Tsuuiisou);
        }

        if (Chinroutou.Valid(hand, []))
        {
            yakuList = yakuList.Add(Yaku.Chinroutou);
        }

        if (Daisharin.Valid(hand, gameRules))
        {
            yakuList = yakuList.Add(Yaku.Daisharin);
        }

        return yakuList;
    }
}
