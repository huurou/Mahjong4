namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// 天鳳 JSON 牌譜の支払形状。
/// Ron / 子ツモ / 親ツモ で scoreText の末尾点数表記が異なるため区別する
/// </summary>
public abstract record ScorePaymentShape;

/// <summary>ロン和了: winner が放銃者から受け取る総額を保持</summary>
public sealed record RonPaymentShape(int Points) : ScorePaymentShape;

/// <summary>子ツモ和了: 非親 (子 2 人) と親 1 人の支払分を保持</summary>
public sealed record ChildTsumoPaymentShape(int NonDealerPay, int DealerPay) : ScorePaymentShape;

/// <summary>親ツモ和了: 全子が同額支払うため 1 名分だけ保持</summary>
public sealed record DealerTsumoPaymentShape(int EachPay) : ScorePaymentShape;

/// <summary>
/// 天鳳 JSON 牌譜の score_text (和了詳細の符翻点文字列) を生成する
/// </summary>
/// <remarks>
/// 形式 (tenhou.net/6 公式ログ準拠):
/// <list type="bullet">
/// <item>Ron: <c>"{prefix}{points}点"</c> (例 <c>"30符2飜2900点"</c> / <c>"満貫8000点"</c>)</item>
/// <item>子ツモ: <c>"{prefix}{nonDealerPay}-{dealerPay}点"</c> (例 <c>"30符4飜2000-3900点"</c> / <c>"満貫2000-4000点"</c>)</item>
/// <item>親ツモ: <c>"{prefix}{eachPay}点∀"</c> (例 <c>"30符4飜3900点∀"</c> / <c>"満貫4000点∀"</c>)</item>
/// </list>
/// prefix は階級判定:
/// <list type="bullet">
/// <item>役満 (IsYakuman=true, yakumanMultiplier=N): N=1→"役満" / N≥2→"{漢数字 N}倍役満"</item>
/// <item>Han≥13 かつ非役満: "数え役満"</item>
/// <item>Han 11-12: "三倍満" / Han 8-10: "倍満" / Han 6-7: "跳満"</item>
/// <item>Han=5 / Han=4 かつ Fu≥40 / Han=3 かつ Fu≥70: "満貫"</item>
/// <item>それ以外: "{Fu}符{Han}飜"</item>
/// </list>
/// </remarks>
public static class TenhouScoreTextFormatter
{
    private static readonly string[] KANJI_DIGITS = ["", "", "二", "三", "四", "五", "六"];

    /// <summary>
    /// 和了の符翻・支払形状から天鳳 score_text を生成する
    /// </summary>
    /// <param name="han">翻数</param>
    /// <param name="fu">符数</param>
    /// <param name="shape">支払形状 (Ron / 子ツモ / 親ツモ)</param>
    /// <param name="isYakuman">役満か</param>
    /// <param name="yakumanMultiplier">役満の重ね倍数 (1=単独役満, 2=ダブル役満, ...)。非役満時は無視</param>
    public static string Format(int han, int fu, ScorePaymentShape shape, bool isYakuman, int yakumanMultiplier)
    {
        var prefix = BuildPrefix(han, fu, isYakuman, yakumanMultiplier);
        return shape switch
        {
            RonPaymentShape ron => $"{prefix}{ron.Points}点",
            ChildTsumoPaymentShape child => $"{prefix}{child.NonDealerPay}-{child.DealerPay}点",
            DealerTsumoPaymentShape dealer => $"{prefix}{dealer.EachPay}点∀",
            _ => throw new ArgumentException($"未知の ScorePaymentShape: {shape.GetType()}", nameof(shape)),
        };
    }

    private static string BuildPrefix(int han, int fu, bool isYakuman, int yakumanMultiplier)
    {
        if (isYakuman)
        {
            return yakumanMultiplier <= 1
                ? "役満"
                : $"{KANJI_DIGITS[yakumanMultiplier]}倍役満";
        }
        return han >= 13 ? "数え役満"
            : han >= 11 ? "三倍満"
            : han >= 8 ? "倍満"
            : han >= 6 ? "跳満"
            : han == 5 || (han == 4 && fu >= 40) || (han == 3 && fu >= 70) ? "満貫"
            : $"{fu}符{han}飜";
    }
}
