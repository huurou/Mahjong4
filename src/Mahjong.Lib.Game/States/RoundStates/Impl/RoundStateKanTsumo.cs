using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

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
        // Rinshan の和了牌 = 和了者の手牌末尾 (直前に嶺上からツモった牌)
        var winTile = context.Round.HandArray[context.Round.Turn].Last();
        var (settledRound, details) = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, winTile, context.ScoreCalculator);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType, details.Winners, details.Honba, details.KyoutakuRiichiAward);
        Transit(context, new RoundStateWin(eventArgs), _ => settledRound);
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            var candidates = playerIndex == round.Turn
                ? enumerator.EnumerateForKanTsumo(round, playerIndex)
                : new CandidateList([new OkCandidate()]);
            specs.Add(new PlayerInquirySpec(playerIndex, candidates));
        }
        return new RoundInquirySpec(RoundInquiryPhase.KanTsumo, specs.ToImmutable(), [round.Turn], round.Turn);
    }
}
