using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Inquiries;

/// <summary>
/// 局面ごとのプレイヤー問い合わせ仕様 (誰に・何を聞くか)
/// RoundState が生成し RoundStateContext の通知・応答集約ループが通知・応答に変換する
/// <para>
/// 通知自体は全プレイヤーへ送信される (表示目的)。そのうち「問い合わせ対象」
/// (= 意味のある入力を期待するプレイヤー) は <see cref="InquiredPlayerIndices"/>
/// で明示する。非対象プレイヤーの <see cref="PlayerInquirySpec.CandidateList"/> は
/// <c>[OkCandidate]</c> のみで、非 OK 応答を返した場合は例外となる
/// </para>
/// </summary>
/// <param name="Phase">問い合わせフェーズ</param>
/// <param name="PlayerSpecs">各プレイヤーへの問い合わせ仕様 (常に 4 人分)</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー
/// (Tsumo/KanTsumo/AfterKanTsumo: 手番 1 人 / Dahai/Kan: 非手番 3 人 / 観測フェーズ: 空)</param>
/// <param name="LoserIndex">放銃者候補 (打牌局面は打牌者 / 槓局面は加槓者)。
/// 優先順位ポリシーの並び順 (天鳳: 放銃者から見た上家優先) 判定に用いる。
/// Dahai/Kan 以外のフェーズでは優先順位解決に使われないため、便宜上 <see cref="Round.Turn"/> を入れる</param>
public record RoundInquirySpec(
    RoundInquiryPhase Phase,
    ImmutableList<PlayerInquirySpec> PlayerSpecs,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices,
    PlayerIndex LoserIndex
)
{
    /// <summary>
    /// 指定プレイヤーが問い合わせ対象かを返す
    /// </summary>
    public bool IsInquired(PlayerIndex playerIndex)
    {
        return InquiredPlayerIndices.Contains(playerIndex);
    }
}
