using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓ツモ
/// </summary>
public record RoundStateKanTsumo : RoundState
{
    public override string Name => "槓ツモ";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        // TODO: 四槓流れ判定 (4人目の槓で流局)
        Transit(context, new RoundStateAfterKanTsumo());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        if (evt.WinType is not WinType.Rinshan)
        {
            throw new InvalidOperationException($"槓ツモ状態からの和了応答は Rinshan のみ。実際:{evt.WinType}");
        }

        // 嶺上開花 (ツモ和了、Loser は和了者自身)
        var loserIndex = context.Round.Turn;
        var settledRound = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, context.ScoreCalculator, out var details);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType, details.Winners, details.Honba, details.KyoutakuRiichiAward);
        Transit(context, new RoundStateWin(eventArgs), () => context.Round = settledRound);
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var spec = new PlayerInquirySpec(round.Turn, enumerator.EnumerateForKanTsumo(round, round.Turn));
        return new RoundInquirySpec(RoundInquiryPhase.KanTsumo, [spec], null);
    }
}
