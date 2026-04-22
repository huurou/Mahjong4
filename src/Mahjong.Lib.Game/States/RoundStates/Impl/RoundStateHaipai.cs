using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 配牌
/// </summary>
public record RoundStateHaipai : RoundState
{
    public override string Name => "配牌";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        Transit(context, () => new RoundStateTsumo(), round => round.Tsumo());
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            specs.Add(new PlayerInquirySpec(new PlayerIndex(i), [new OkCandidate()]));
        }
        return new RoundInquirySpec(RoundInquiryPhase.Haipai, specs.ToImmutable(), [], round.Turn);
    }
}
