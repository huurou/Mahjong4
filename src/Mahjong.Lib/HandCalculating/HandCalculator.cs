using Mahjong.Lib.Calls;
using Mahjong.Lib.Fus;
using Mahjong.Lib.Games;
using Mahjong.Lib.HandCalculating.HandDividing;
using Mahjong.Lib.HandCalculating.Scores;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using System.Diagnostics.CodeAnalysis;

namespace Mahjong.Lib.HandCalculating;

/// <summary>
/// 麻雀の手牌から役と点数の計算を行うクラス
/// </summary>
public static class HandCalculator
{
    /// <summary>
    /// 手牌・状況・ルールをもとに和了結果を計算します。
    /// </summary>
    /// <param name="tileKindList">手牌の牌リスト 副露を含まない手牌</param>
    /// <param name="winTile">和了牌（天和時,流し満貫時はnull）</param>
    /// <param name="callList">副露リスト</param>
    /// <param name="doraIndicators">ドラ表示牌リスト</param>
    /// <param name="uradoraIndicators">裏ドラ表示牌リスト</param>
    /// <param name="winSituation">和了状況</param>
    /// <param name="gameRules">ルール設定</param>
    /// <returns>和了結果</returns>
    public static HandResult Calc(
        TileKindList tileKindList,
        TileKind? winTile,
        CallList? callList = null,
        TileKindList? doraIndicators = null,
        TileKindList? uradoraIndicators = null,
        WinSituation? winSituation = null,
        GameRules? gameRules = null
    )
    {
        callList ??= [];
        doraIndicators ??= [];
        uradoraIndicators ??= [];
        winSituation ??= new();
        gameRules ??= new();

        // エラーチェック（特殊役の判定より先に実行する）
        if (HandValidator.HasError(tileKindList, winTile, callList, winSituation, out var errorResult))
        {
            return errorResult;
        }

        // 特殊役の判定（流し満貫・国士無双）
        if (TryEvaluateSpecialHands(tileKindList, winTile, winSituation, gameRules, out var specialResult))
        {
            return specialResult;
        }

        // 通常の手牌計算
        var handResults = CalculateNormalHands(tileKindList, winTile, callList, doraIndicators, uradoraIndicators, winSituation, gameRules);

        // 高点法に基づき最高点の結果を返す
        return SelectBestResult(handResults);
    }

    /// <summary>
    /// 特殊役の早期判定を試行します。
    /// </summary>
    private static bool TryEvaluateSpecialHands(
        TileKindList tileKindList,
        TileKind? winTile,
        WinSituation winSituation,
        GameRules gameRules,
        [NotNullWhen(true)] out HandResult? result
    )
    {
        // 流し満貫の判定
        if (SpecialHandEvaluator.EvaluateNagashimangan(winSituation, winTile, out result))
        {
            return true;
        }

        // 国士無双の判定
        if (SpecialHandEvaluator.EvaluateKokushimusou(tileKindList, winTile, winSituation, gameRules, out result))
        {
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// 通常の手牌計算を行います。
    /// </summary>
    private static List<HandResult> CalculateNormalHands(
        TileKindList tileKindList,
        TileKind? winTile,
        CallList callList,
        TileKindList doraIndicators,
        TileKindList uradoraIndicators,
        WinSituation winSituation,
        GameRules gameRules
    )
    {
        var hands = HandDivider.Divide(tileKindList);
        var handResults = new List<HandResult>();

        foreach (var hand in hands)
        {
            // 天和は和了牌がnullになるので、特別に判定する
            if (winTile is null)
            {
                ProcessTenhouHand(hand, callList, winSituation, gameRules, handResults);
                continue;
            }

            ProcessNormalHand(hand, winTile, callList, doraIndicators, uradoraIndicators, winSituation, gameRules, handResults);
        }

        return handResults;
    }

    /// <summary>
    /// 天和の手牌を処理します。（winTile==null は HandValidator で天和のみ許可済み）
    /// </summary>
    private static void ProcessTenhouHand(
        Hand hand,
        CallList callList,
        WinSituation winSituation,
        GameRules gameRules,
        List<HandResult> handResults
    )
    {
        var yakuList = SpecialHandEvaluator.EvaluateTenhou(hand, winSituation, gameRules);
        handResults.Add(HandResult.Create(yakuList, callList: callList, winSituation: winSituation, gameRules: gameRules));
    }

    /// <summary>
    /// 通常の手牌を処理します
    /// </summary>
    private static void ProcessNormalHand(
        Hand hand,
        TileKind winTile,
        CallList callList,
        TileKindList doraIndicators,
        TileKindList uradoraIndicators,
        WinSituation winSituation,
        GameRules gameRules,
        List<HandResult> handResults
    )
    {
        var winGroups = hand.GetWinGroups(winTile);

        foreach (var winGroup in winGroups)
        {
            var fuList = FuCalculator.Calc(hand, winTile, winGroup, callList, winSituation, gameRules);
            var yakuList = YakuEvaluator.EvaluateYaku(hand, winTile, winGroup, callList, fuList, winSituation, gameRules);

            yakuList = ProcessYakuList(yakuList, hand, callList, doraIndicators, uradoraIndicators, winSituation);

            AddHandResult(yakuList, fuList, callList, winSituation, gameRules, handResults);
        }
    }

    /// <summary>
    /// 役リストを処理し、役満とドラを適切に扱います。
    /// </summary>
    private static YakuList ProcessYakuList(
        YakuList yakuList,
        Hand hand,
        CallList callList,
        TileKindList doraIndicators,
        TileKindList uradoraIndicators,
        WinSituation winSituation
    )
    {
        if (yakuList.Any(x => x.IsYakuman))
        {
            // 役満が含まれていた場合は役満だけにする
            return [.. yakuList.Where(x => x.IsYakuman)];
        }

        if (yakuList.Count != 0)
        {
            return yakuList.AddRange(YakuEvaluator.EvaluateDora(hand, callList, doraIndicators, uradoraIndicators, winSituation));
        }

        return yakuList;
    }

    /// <summary>
    /// 手牌結果をリストに追加します。
    /// </summary>
    private static void AddHandResult(
        YakuList yakuList,
        FuList fuList,
        CallList callList,
        WinSituation winSituation,
        GameRules gameRules,
        List<HandResult> handResults
    )
    {
        handResults.Add(yakuList.Count != 0
            ? HandResult.Create(yakuList, fuList, callList, winSituation, gameRules)
            : HandResult.Error("役がありません。")
        );
    }

    /// <summary>
    /// 高点法に基づき最高の結果を選択します。
    /// </summary>
    /// <remarks>
    /// HandValidator で和了形が保証されているため handResults は必ず1件以上含む
    /// </remarks>
    private static HandResult SelectBestResult(List<HandResult> handResults)
    {
        return handResults
            .OrderByDescending(x => TotalScore(x.Score))
            .ThenByDescending(x => x.Han)
            .ThenByDescending(x => x.Fu)
            .First();
    }

    /// <summary>
    /// Scoreから合計支払い点数を算出します。
    /// </summary>
    private static int TotalScore(Score score)
    {
        return score.Main + score.Sub * 2;
    }
}
