using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 副露。副露直後に全プレイヤーへ CallNotification を送り OK 応答を集約する観測点。
/// ResponseOk 受信で次状態 (副露後 or 嶺上ツモ) へ遷移する
/// </summary>
public record RoundStateCall : RoundState
{
    public override string Name => "副露";

    /// <summary>
    /// 副露直後・後続の RinshanTsumo 実行前の Round スナップショット。
    /// 大明槓時は本状態の後に RinshanTsumo() を伴う KanTsumo へ遷移するため、
    /// RoundStateContext が副露通知を送る時点では context.Round が既に RinshanTsumo 後になる可能性がある。
    /// 副露通知の整合を保つため遷移時に副露直後の Round を封入する
    /// </summary>
    public Round? SnapshotRound { get; init; }

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        if (IsDaiminkan(context))
        {
            Transit(context, () => new RoundStateKanTsumo(), round => round.RinshanTsumo());
        }
        else
        {
            // ポン/チー後は副露者 (= 現手番) の打牌のみ受け付けるため、専用の RoundStateAfterCall に遷移する
            Transit(context, () => new RoundStateAfterCall());
        }
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            specs.Add(new PlayerInquirySpec(new PlayerIndex(i), [new OkCandidate()]));
        }
        return new RoundInquirySpec(RoundInquiryPhase.Call, specs.ToImmutable(), [], round.Turn);
    }

    private static bool IsDaiminkan(RoundStateContext context)
    {
        var lastCall = context.Round.CallListArray[context.Round.Turn].LastOrDefault();
        return lastCall?.Type == CallType.Daiminkan;
    }
}
