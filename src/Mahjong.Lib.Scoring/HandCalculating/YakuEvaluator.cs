using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Fus;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.HandCalculating;

/// <summary>
/// 通常の役の判定を行います
/// </summary>
internal static class YakuEvaluator
{
    /// <summary>
    /// 役の判定を行う
    /// </summary>
    internal static YakuList EvaluateYaku(Hand hand, TileKind winTile, TileKindList winGroup, CallList callList, FuList fuList, WinSituation winSituation, GameRules gameRules)
    {
        var yakuList = new YakuList();

        yakuList = yakuList.AddRange(EvaluateFormlessYaku(hand, callList, winSituation, gameRules));

        if (hand.Count == 7)
        {
            yakuList = yakuList.AddRange(EvaluateChiitoitsuYaku(hand, gameRules));
        }

        if (hand.CombineFuuro(callList).Any(x => x.IsShuntsu))
        {
            yakuList = yakuList.AddRange(EvaluateShuntsuYaku(hand, callList, fuList, winSituation, gameRules));
        }

        if (hand.CombineFuuro(callList).Any(x => x.IsKoutsu || x.IsKantsu))
        {
            yakuList = yakuList.AddRange(EvaluateKoutsuYaku(hand, winTile, winGroup, callList, winSituation, gameRules));
        }

        return yakuList;
    }

    /// <summary>
    /// ドラの判定を行う
    /// </summary>
    internal static YakuList EvaluateDora(Hand hand, CallList callList, TileKindList doraIndicators, TileKindList uradoraIndicators, WinSituation winSituation)
    {
        var yakuList = new YakuList();
        var tiles = hand.Concat(callList.TileKindLists).SelectMany(x => x).ToList();

        var doraCount = CountMatchingTiles(tiles, doraIndicators);
        yakuList = yakuList.AddRange(Enumerable.Repeat(Yaku.Dora, doraCount));

        var uradoraCount = CountMatchingTiles(tiles, uradoraIndicators);
        yakuList = yakuList.AddRange(Enumerable.Repeat(Yaku.Uradora, uradoraCount));

        yakuList = yakuList.AddRange(Enumerable.Repeat(Yaku.Akadora, winSituation.AkadoraCount));

        return yakuList;
    }

    /// <summary>
    /// ドラ表示牌に対応する実ドラの個数を数える
    /// </summary>
    private static int CountMatchingTiles(List<TileKind> tiles, TileKindList indicators)
    {
        return indicators.Select(TileKind.GetActualDora).Sum(x => tiles.Count(y => x == y));
    }

    /// <summary>
    /// 形が関係ない役の判定を行う
    /// </summary>
    private static YakuList EvaluateFormlessYaku(Hand hand, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        var yakuList = new YakuList();

        // 基本的な状況役
        yakuList = AddBasicSituationYaku(yakuList, callList, winSituation, gameRules);

        // 牌の種類に関する役
        yakuList = AddTileTypeYaku(yakuList, hand, callList, gameRules);

        // 特殊状況役
        yakuList = AddSpecialSituationYaku(yakuList, winSituation, gameRules);

        return yakuList;
    }

    /// <summary>
    /// 基本的な状況役を追加します
    /// </summary>
    private static YakuList AddBasicSituationYaku(YakuList yakuList, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        if (Tsumo.Valid(callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.Tsumo);
        }

        if (Riichi.Valid(winSituation, callList))
        {
            yakuList = yakuList.Add(Yaku.Riichi);
        }

        if (DoubleRiichi.Valid(winSituation, callList))
        {
            yakuList = yakuList.Add(Yaku.DoubleRiichi);
        }

        if (Ippatsu.Valid(winSituation, callList))
        {
            yakuList = yakuList.Add(Yaku.Ippatsu);
        }

        if (Chankan.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Chankan);
        }

        if (Rinshan.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Rinshan);
        }

        if (Haitei.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Haitei);
        }

        if (Houtei.Valid(winSituation))
        {
            yakuList = yakuList.Add(Yaku.Houtei);
        }

        if (Renhou.Valid(winSituation, gameRules))
        {
            yakuList = yakuList.Add(Yaku.Renhou);
        }

        return yakuList;
    }

    /// <summary>
    /// 牌の種類に関する役を追加します
    /// </summary>
    private static YakuList AddTileTypeYaku(YakuList yakuList, Hand hand, CallList callList, GameRules gameRules)
    {
        if (Tanyao.Valid(hand, callList, gameRules))
        {
            yakuList = yakuList.Add(Yaku.Tanyao);
        }

        if (Honitsu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Honitsu);
        }

        if (Chinitsu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Chinitsu);
        }

        if (Tsuuiisou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Tsuuiisou);
        }

        if (Honroutou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Honroutou);
        }

        if (Ryuuiisou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Ryuuiisou);
        }

        return yakuList;
    }

    /// <summary>
    /// 特殊状況役を追加します
    /// </summary>
    private static YakuList AddSpecialSituationYaku(YakuList yakuList, WinSituation winSituation, GameRules gameRules)
    {
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
    /// 七対子形の役の判定を行う
    /// </summary>
    private static YakuList EvaluateChiitoitsuYaku(Hand hand, GameRules gameRules)
    {
        var yakuList = new YakuList();

        if (Chiitoitsu.Valid(hand))
        {
            yakuList = yakuList.Add(Yaku.Chiitoitsu);
        }

        if (Daisharin.Valid(hand, gameRules))
        {
            yakuList = yakuList.Add(Yaku.Daisharin);
        }

        return yakuList;
    }

    /// <summary>
    /// 順子が必要な役の判定を行う
    /// </summary>
    private static YakuList EvaluateShuntsuYaku(Hand hand, CallList callList, FuList fuList, WinSituation winSituation, GameRules gameRules)
    {
        var yakuList = new YakuList();

        if (Pinfu.Valid(fuList, winSituation, gameRules))
        {
            yakuList = yakuList.Add(Yaku.Pinfu);
        }

        yakuList = AddSequenceBasedYaku(yakuList, hand, callList);

        return yakuList;
    }

    /// <summary>
    /// 順子ベースの役を追加します
    /// </summary>
    private static YakuList AddSequenceBasedYaku(YakuList yakuList, Hand hand, CallList callList)
    {
        if (Chanta.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Chanta);
        }

        if (Junchan.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Junchan);
        }

        if (Ittsuu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Ittsuu);
        }

        // 一盃口と二盃口は両立しない
        if (Ryanpeikou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Ryanpeikou);
        }
        else if (Iipeikou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Iipeikou);
        }

        if (Sanshoku.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Sanshoku);
        }

        return yakuList;
    }

    /// <summary>
    /// 刻子が必要な役の判定を行う
    /// </summary>
    private static YakuList EvaluateKoutsuYaku(Hand hand, TileKind winTile, TileKindList winGroup, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        var yakuList = new YakuList();

        yakuList = AddBasicKoutsuYaku(yakuList, hand, winGroup, callList, winSituation);
        yakuList = AddHonorTileYaku(yakuList, hand, callList, winSituation);
        yakuList = AddYakumanKoutsuYaku(yakuList, hand, winTile, winGroup, callList, winSituation, gameRules);

        return yakuList;
    }

    /// <summary>
    /// 基本的な刻子役を追加します
    /// </summary>
    private static YakuList AddBasicKoutsuYaku(YakuList yakuList, Hand hand, TileKindList winGroup, CallList callList, WinSituation winSituation)
    {
        if (Toitoihou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Toitoihou);
        }

        if (Sanankou.Valid(hand, winGroup, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.Sanankou);
        }

        if (Sanshokudoukou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Sanshokudoukou);
        }

        if (Sankantsu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Sankantsu);
        }

        return yakuList;
    }

    /// <summary>
    /// 字牌役を追加します
    /// </summary>
    private static YakuList AddHonorTileYaku(YakuList yakuList, Hand hand, CallList callList, WinSituation winSituation)
    {
        // 三元牌
        if (Shousangen.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Shousangen);
        }

        if (Haku.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Haku);
        }

        if (Hatsu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Hatsu);
        }

        if (Chun.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Chun);
        }

        // 自風牌
        yakuList = AddPlayerWindYaku(yakuList, hand, callList, winSituation);

        // 場風牌
        yakuList = AddRoundWindYaku(yakuList, hand, callList, winSituation);

        return yakuList;
    }

    /// <summary>
    /// 自風牌役を追加します
    /// </summary>
    private static YakuList AddPlayerWindYaku(YakuList yakuList, Hand hand, CallList callList, WinSituation winSituation)
    {
        if (PlayerWindEast.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.PlayerWindEast);
        }

        if (PlayerWindSouth.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.PlayerWindSouth);
        }

        if (PlayerWindWest.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.PlayerWindWest);
        }

        if (PlayerWindNorth.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.PlayerWindNorth);
        }

        return yakuList;
    }

    /// <summary>
    /// 場風牌役を追加します
    /// </summary>
    private static YakuList AddRoundWindYaku(YakuList yakuList, Hand hand, CallList callList, WinSituation winSituation)
    {
        if (RoundWindEast.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.RoundWindEast);
        }

        if (RoundWindSouth.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.RoundWindSouth);
        }

        if (RoundWindWest.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.RoundWindWest);
        }

        if (RoundWindNorth.Valid(hand, callList, winSituation))
        {
            yakuList = yakuList.Add(Yaku.RoundWindNorth);
        }

        return yakuList;
    }

    /// <summary>
    /// 役満の刻子役を追加します
    /// </summary>
    private static YakuList AddYakumanKoutsuYaku(YakuList yakuList, Hand hand, TileKind winTile, TileKindList winGroup, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        yakuList = AddSangenYakuman(yakuList, hand, callList);
        yakuList = AddSuushiiYakuman(yakuList, hand, callList, gameRules);
        yakuList = AddSuuankouYakuman(yakuList, hand, winTile, winGroup, callList, winSituation, gameRules);
        yakuList = AddChuurenpoutouYakuman(yakuList, hand, winTile, gameRules);
        yakuList = AddOtherYakuman(yakuList, hand, callList);

        return yakuList;
    }

    /// <summary>
    /// 三元系役満を追加します
    /// </summary>
    private static YakuList AddSangenYakuman(YakuList yakuList, Hand hand, CallList callList)
    {
        if (Daisangen.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Daisangen);
        }

        return yakuList;
    }

    /// <summary>
    /// 四喜系役満を追加します
    /// </summary>
    private static YakuList AddSuushiiYakuman(YakuList yakuList, Hand hand, CallList callList, GameRules gameRules)
    {
        if (Shousuushii.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Shousuushii);
        }

        if (Daisuushii.Valid(hand, callList))
        {
            if (DaisuushiiDouble.Valid(hand, callList, gameRules))
            {
                yakuList = yakuList.Add(Yaku.DaisuushiiDouble);
            }
            else
            {
                yakuList = yakuList.Add(Yaku.Daisuushii);
            }
        }

        return yakuList;
    }

    /// <summary>
    /// 四暗刻系役満を追加します
    /// </summary>
    private static YakuList AddSuuankouYakuman(YakuList yakuList, Hand hand, TileKind winTile, TileKindList winGroup, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        if (Suuankou.Valid(hand, winGroup, callList, winSituation))
        {
            if (SuuankouTanki.Valid(hand, winGroup, winTile, callList, winSituation))
            {
                if (SuuankouTankiDouble.Valid(hand, winGroup, winTile, callList, winSituation, gameRules))
                {
                    yakuList = yakuList.Add(Yaku.SuuankouTankiDouble);
                }
                else
                {
                    yakuList = yakuList.Add(Yaku.SuuankouTanki);
                }
            }
            else
            {
                yakuList = yakuList.Add(Yaku.Suuankou);
            }
        }

        return yakuList;
    }

    /// <summary>
    /// 九蓮宝燈系役満を追加します
    /// </summary>
    private static YakuList AddChuurenpoutouYakuman(YakuList yakuList, Hand hand, TileKind winTile, GameRules gameRules)
    {
        if (Chuurenpoutou.Valid(hand))
        {
            if (JunseiChuurenpoutou.Valid(hand, winTile))
            {
                if (JunseiChuurenpoutouDouble.Valid(hand, winTile, gameRules))
                {
                    yakuList = yakuList.Add(Yaku.JunseiChuurenpoutouDouble);
                }
                else
                {
                    yakuList = yakuList.Add(Yaku.JunseiChuurenpoutou);
                }
            }
            else
            {
                yakuList = yakuList.Add(Yaku.Chuurenpoutou);
            }
        }

        return yakuList;
    }

    /// <summary>
    /// その他の役満を追加します
    /// </summary>
    private static YakuList AddOtherYakuman(YakuList yakuList, Hand hand, CallList callList)
    {
        if (Chinroutou.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Chinroutou);
        }

        if (Suukantsu.Valid(hand, callList))
        {
            yakuList = yakuList.Add(Yaku.Suukantsu);
        }

        return yakuList;
    }
}
