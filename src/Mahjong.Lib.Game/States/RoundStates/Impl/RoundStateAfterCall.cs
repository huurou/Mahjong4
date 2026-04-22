using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 副露後 (チー/ポン直後)
/// 副露者 (= 現手番) に打牌を求める。
/// </summary>
public record RoundStateAfterCall : RoundState
{
    public override string Name => "副露後";

    public override void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt)
    {
        base.ResponseDahai(context, evt);
        if (evt.IsRiichi)
        {
            // 副露中は門前が崩れているため立直不可 (到達しない想定だが念のため拒否)
            throw new InvalidOperationException("副露後に立直は宣言できません。");
        }
        Transit(context, () => new RoundStateDahai(), round => round.Dahai(evt.Tile));
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            var candidates = playerIndex == round.Turn
                ? enumerator.EnumerateForAfterCall(round, playerIndex)
                : new CandidateList([new OkCandidate()]);
            specs.Add(new PlayerInquirySpec(playerIndex, candidates));
        }
        return new RoundInquirySpec(RoundInquiryPhase.AfterCall, specs.ToImmutable(), [round.Turn], round.Turn);
    }
}
