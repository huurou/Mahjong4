using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 天鳳ルール準拠の既定応答生成実装
/// Haipai / Dahai (スルー) / Kan (スルー): OK / Tsumo・KanTsumo・AfterKanTsumo: 先頭 DahaiCandidate を打牌
/// </summary>
public sealed class DefaultResponseFactory : IDefaultResponseFactory
{
    public PlayerResponse CreateDefault(PlayerInquirySpec spec, RoundInquiryPhase phase)
    {
        ArgumentNullException.ThrowIfNull(spec);

        return phase switch
        {
            RoundInquiryPhase.Haipai => new OkResponse(),
            RoundInquiryPhase.Dahai => new OkResponse(),
            RoundInquiryPhase.Kan => new OkResponse(),
            RoundInquiryPhase.Tsumo => CreateDefaultTsumo(spec),
            RoundInquiryPhase.KanTsumo => CreateDefaultKanTsumo(spec),
            RoundInquiryPhase.AfterKanTsumo => CreateDefaultAfterKanTsumo(spec),
            _ => throw new InvalidOperationException($"未対応のフェーズです。実際:{phase}"),
        };
    }

    private static DahaiResponse CreateDefaultTsumo(PlayerInquirySpec spec)
    {
        var dahai = spec.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        return dahai is not null && dahai.DahaiOptionList.Count != 0
            ? new DahaiResponse(dahai.DahaiOptionList[0].Tile)
            : throw new InvalidOperationException("ツモフェーズでは DahaiCandidate が必須ですが、候補が提示されていません。");
    }

    private static KanTsumoDahaiResponse CreateDefaultKanTsumo(PlayerInquirySpec spec)
    {
        var dahai = spec.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        return dahai is not null && dahai.DahaiOptionList.Count != 0
            ? new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile)
            : throw new InvalidOperationException("嶺上ツモフェーズでは DahaiCandidate が必須ですが、候補が提示されていません。");
    }

    private static KanTsumoDahaiResponse CreateDefaultAfterKanTsumo(PlayerInquirySpec spec)
    {
        var dahai = spec.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        return dahai is not null && dahai.DahaiOptionList.Count != 0
            ? new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile)
            : throw new InvalidOperationException("嶺上ツモ後フェーズでは DahaiCandidate が必須ですが、候補が提示されていません。");
    }
}
