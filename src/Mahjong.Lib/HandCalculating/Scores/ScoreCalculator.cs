using Mahjong.Lib.Games;

namespace Mahjong.Lib.HandCalculating.Scores;

/// <summary>
/// 麻雀の点数計算を行うクラス
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// 基本満貫点
    /// </summary>
    private const int MANGAN_BASE_POINTS = 2000;

    /// <summary>
    /// 切り上げ満貫の条件（翻と符）
    /// </summary>
    private static (int han, int fu)[] KiriageConditions => [(4, 30), (3, 60)];

    /// <summary>
    /// 符と翻数から点数を計算します
    /// </summary>
    /// <param name="fu">符</param>
    /// <param name="han">翻</param>
    /// <param name="winSituation">和了の状況</param>
    /// <param name="gameRules">ゲームルール</param>
    /// <param name="isYakuman">役満かどうか ダブル役満 トリプル役満などの場合もtrue</param>
    /// <returns>計算された点数</returns>
    /// <exception cref="ArgumentException">翻が0以下の場合にスロー</exception>
    public static Score Calc(int fu, int han, WinSituation winSituation, GameRules gameRules, bool isYakuman = false)
    {
        if (han <= 0) { throw new ArgumentException("翻は1以上を指定してください。", nameof(han)); }
        // 役満でないときのみ数え役満のルールを適用する
        if (!isYakuman && han >= 13)
        {
            han = gameRules.KazoeLimit switch
            {
                KazoeLimit.Limited => 13,
                KazoeLimit.Sanbaiman => 12,
                KazoeLimit.NoLimit => han,
                _ => throw new ArgumentException("数え役満のルールが不正です。", nameof(gameRules)),
            };
        }
        // 点数計算
        var basePoint = CalculateBasePoint(fu, han, gameRules.KiriageEnabled);
        // 親/子、ツモ/ロンに応じて最終的な点数を計算
        return CalculateFinalScore(basePoint, winSituation);
    }

    /// <summary>
    /// 符と翻から基本点（一家あたりの点数）を計算します
    /// </summary>
    private static int CalculateBasePoint(int fu, int han, bool kiriage)
    {
        if (han >= 5)
        {
            return CalculateFixedBasePoint(han);
        }
        else
        {
            return CalculateVariableBasePoint(fu, han, kiriage);
        }
    }

    /// <summary>
    /// 5翻以上の場合の固定点計算
    /// </summary>
    private static int CalculateFixedBasePoint(int han)
    {
        return han == 5 ? MANGAN_BASE_POINTS // 満貫
            : han <= 7 ? 3000 // 跳満
            : han <= 10 ? 4000 // 倍満
            : han <= 12 ? 6000 // 三倍満
            : Math.Min(han / 13, 6) * 8000; // 役満以降は13翻単位で倍率が増え、六倍役満(78翻)で頭打ちする
    }

    /// <summary>
    /// 1-4翻の場合の点数計算
    /// </summary>
    private static int CalculateVariableBasePoint(int fu, int han, bool kiriage)
    {
        // 基本点計算（符×2^(翻+2)）
        var basePoint = fu * (int)Math.Pow(2, han + 2);
        var rounded = (basePoint + 99) / 100 * 100;
        // 切り上げ満貫の判定
        var isKiriage = kiriage && KiriageConditions.Contains((han, fu));
        return rounded > MANGAN_BASE_POINTS || isKiriage ? MANGAN_BASE_POINTS : basePoint;
    }

    /// <summary>
    /// 最終的な点数を計算します
    /// </summary>
    private static Score CalculateFinalScore(int basePoint, WinSituation winSituation)
    {
        var rounded = (basePoint + 99) / 100 * 100;
        var doubleRounded = (2 * basePoint + 99) / 100 * 100;
        var fourRounded = (4 * basePoint + 99) / 100 * 100;
        var sixRounded = (6 * basePoint + 99) / 100 * 100;
        return winSituation.IsTsumo
            ? winSituation.IsDealer
                // 親のツモ：子全員が基本点×2点支払う
                ? new(doubleRounded, doubleRounded)
                // 子のツモ：親は基本点×2点、子は基本点×1点支払う
                : new(doubleRounded, rounded)
            : winSituation.IsDealer
                // 親のロン：子は基本点×6点支払う
                ? new(sixRounded)
                // 子のロン：親は基本点×4点支払う
                : new(fourRounded);
    }
}
