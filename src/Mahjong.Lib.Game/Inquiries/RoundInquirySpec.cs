using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Inquiries;

/// <summary>
/// 局面ごとのプレイヤー問い合わせ仕様 (誰に・何を聞くか)
/// RoundState が生成し RoundManager が通知・応答に変換する
/// </summary>
/// <param name="Phase">問い合わせフェーズ</param>
/// <param name="PlayerSpecs">各プレイヤーへの問い合わせ仕様</param>
/// <param name="LoserIndex">放銃者候補 (打牌局面は打牌者 / 槓局面は加槓者 / それ以外は null)。
/// 優先順位ポリシーの並び順 (天鳳: 放銃者から見た上家優先) 判定に用いる</param>
public record RoundInquirySpec(
    RoundInquiryPhase Phase,
    ImmutableList<PlayerInquirySpec> PlayerSpecs,
    PlayerIndex? LoserIndex
);
