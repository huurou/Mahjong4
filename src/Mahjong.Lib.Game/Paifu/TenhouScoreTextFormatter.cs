namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// 天鳳 JSON 牌譜の score_text (和了詳細の符翻点文字列) を生成する
/// </summary>
/// <remarks>
/// mjlog2json / tensoul / tenhou-go 等の OSS 実装に準拠したロジック:
/// <list type="bullet">
/// <item>役満 (IsYakuman=true, yakumanMultiplier=N):
///   N=1→"役満{点}点" / N≥2→"{漢数字 N}倍役満{点}点"</item>
/// <item>Han≥13 かつ非役満: "数え役満{点}点"</item>
/// <item>Han 11-12: "三倍満{点}点"</item>
/// <item>Han 8-10: "倍満{点}点"</item>
/// <item>Han 6-7: "跳満{点}点"</item>
/// <item>Han=5 / Han=4 かつ Fu≥40 / Han=3 かつ Fu≥70: "満貫{点}点"</item>
/// <item>それ以外: "{Fu}符{Han}飜{点}点"</item>
/// </list>
/// </remarks>
public static class TenhouScoreTextFormatter
{
    private static readonly string[] KANJI_DIGITS = ["", "", "二", "三", "四", "五", "六"];

    /// <summary>
    /// 和了の符翻点から天鳳 score_text を生成する
    /// </summary>
    /// <param name="han">翻数</param>
    /// <param name="fu">符数</param>
    /// <param name="points">和了者の獲得合計点 (本場・供託込の正値)</param>
    /// <param name="isYakuman">役満か</param>
    /// <param name="yakumanMultiplier">役満の重ね倍数 (1=単独役満, 2=ダブル役満, ...)。非役満時は無視</param>
    public static string Format(int han, int fu, int points, bool isYakuman, int yakumanMultiplier)
    {
        if (isYakuman)
        {
            return yakumanMultiplier <= 1
                ? $"役満{points}点"
                : $"{KANJI_DIGITS[yakumanMultiplier]}倍役満{points}点";
        }
        if (han >= 13)
        {
            return $"数え役満{points}点";
        }
        if (han >= 11)
        {
            return $"三倍満{points}点";
        }
        if (han >= 8)
        {
            return $"倍満{points}点";
        }
        if (han >= 6)
        {
            return $"跳満{points}点";
        }
        if (han == 5 || (han == 4 && fu >= 40) || (han == 3 && fu >= 70))
        {
            return $"満貫{points}点";
        }

        return $"{fu}符{han}飜{points}点";
    }
}
