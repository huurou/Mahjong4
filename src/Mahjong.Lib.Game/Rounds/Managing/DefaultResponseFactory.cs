using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 天鳳ルール準拠の既定応答生成実装。
/// DahaiCandidate が提示されていないプレイヤー (= 観測のみの問い合わせ対象外) は常に OkResponse。
/// 提示されていればフェーズに応じて打牌系応答 (先頭 DahaiOption をツモ切り) を返す
/// </summary>
public sealed class DefaultResponseFactory : IDefaultResponseFactory
{
    public PlayerResponse CreateDefault(PlayerInquirySpec spec, RoundInquiryPhase phase)
    {
        var dahai = spec.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        if (dahai is null || dahai.DahaiOptionList.Count == 0)
        {
            // DahaiCandidate なし: 観測のみの非問い合わせ対象プレイヤー、または Dahai/Kan 等のスルー既定
            return new OkResponse();
        }

        return phase switch
        {
            RoundInquiryPhase.Tsumo => new DahaiResponse(dahai.DahaiOptionList[0].Tile),
            RoundInquiryPhase.KanTsumo => new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile),
            RoundInquiryPhase.AfterKanTsumo => new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile),
            _ => new OkResponse(),
        };
    }
}
