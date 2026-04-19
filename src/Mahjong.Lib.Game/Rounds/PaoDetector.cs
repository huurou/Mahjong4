using Mahjong.Lib.Game.Calls;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 副露履歴から包 (責任払い) の責任者確定トリガを検出します。
/// 大三元・大四喜は他家から鳴いた刻子/槓子のうち新規牌種を増やす Pon/Daiminkan の種類が確定数に達した瞬間。
/// 加槓は既存ポンを槓に差し替えるだけで新規牌種を増やさないため、大三元/大四喜のトリガ対象外。
/// 四槓子は暗槓を除く槓 (Daiminkan/Kakan) によって総槓数が 4 に達した瞬間。
/// 暗槓由来の役満は包対象外。
///
/// <see cref="Round.Pon"/> / <see cref="Round.Daiminkan"/> / <see cref="Round.Kakan"/> 内で副露履歴更新と同じ
/// <c>with</c> 式で <see cref="Round.PaoResponsibleArray"/> を確定させるため、本検出は Round 内で呼ばれる
/// (副露履歴と責任者更新の atomicity を優先し、RoundState / RoundManager 側には切り出していない)
/// </summary>
public static class PaoDetector
{
    // 字牌の牌種ID: 27=東, 28=南, 29=西, 30=北 (風牌)、31=白, 32=發, 33=中 (三元牌)
    private const int WIND_KIND_MIN = 27;
    private const int WIND_KIND_MAX = 30;
    private const int SANGEN_KIND_MIN = 31;
    private const int SANGEN_KIND_MAX = 33;

    /// <summary>
    /// 指定の副露追加が包役満の確定トリガになるかを判定します。
    /// 戻り値が <see cref="PaoYakuman.None"/> 以外 (<see cref="PaoYakumanExtensions.IsPao"/> で判定可) なら包成立。
    /// 責任者の特定は呼び出し側の責務 (Pon/Daiminkan なら鳴かれた <c>fromIndex</c>、Kakan なら元ポンの <c>From</c>)
    /// </summary>
    /// <param name="callListAfter">副露追加後の CallList</param>
    /// <param name="justAddedCall">今追加された副露</param>
    public static PaoYakuman Detect(CallList callListAfter, Call justAddedCall)
    {
        if (justAddedCall.Type == CallType.Ankan)
        {
            // 暗槓は責任者なし (本人由来)
            return PaoYakuman.None;
        }

        var kind = justAddedCall.Tiles[0].Kind;

        // 大三元 / 大四喜: 新規牌種を増やす Pon/Daiminkan のみトリガ対象。
        // Kakan は既存ポン (= 既に同種3枚) を槓に差し替えるだけで、新規牌種を増やさない。
        // 仮に Kakan 時点で「既に3種揃い」だったとしても、ポン時点で責任者は確定済みであり、
        // Kakan を再トリガとするとポンの from で責任者を上書きしてしまう設計バグになる。
        if (justAddedCall.Type is CallType.Pon or CallType.Daiminkan)
        {
            if (kind is >= SANGEN_KIND_MIN and <= SANGEN_KIND_MAX)
            {
                var sangenKinds = callListAfter
                    .Where(x => x.Type is CallType.Pon or CallType.Daiminkan or CallType.Kakan)
                    .Select(x => x.Tiles[0].Kind)
                    .Where(x => x is >= SANGEN_KIND_MIN and <= SANGEN_KIND_MAX)
                    .Distinct()
                    .Count();
                if (sangenKinds == 3)
                {
                    return PaoYakuman.Daisangen;
                }
            }

            if (kind is >= WIND_KIND_MIN and <= WIND_KIND_MAX)
            {
                var windKinds = callListAfter
                    .Where(x => x.Type is CallType.Pon or CallType.Daiminkan or CallType.Kakan)
                    .Select(x => x.Tiles[0].Kind)
                    .Where(x => x is >= WIND_KIND_MIN and <= WIND_KIND_MAX)
                    .Distinct()
                    .Count();
                if (windKinds == 4)
                {
                    return PaoYakuman.Daisuushii;
                }
            }
        }

        // 四槓子: 最後の槓が他家由来 (Daiminkan/Kakan) で、総槓数 (暗槓含む) が 4 に達したら確定
        if (justAddedCall.Type is CallType.Daiminkan or CallType.Kakan)
        {
            var kanCount = callListAfter
                .Count(x => x.Type is CallType.Ankan or CallType.Daiminkan or CallType.Kakan);
            if (kanCount == 4)
            {
                return PaoYakuman.Suukantsu;
            }
        }

        return PaoYakuman.None;
    }
}
