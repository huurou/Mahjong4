using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Decisions;
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
        Transit(context, new RoundStateTsumo(), () => context.Round = context.Round.Tsumo());
    }

    public override RoundDecisionSpec CreateDecisionSpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerDecisionSpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            specs.Add(new PlayerDecisionSpec(new PlayerIndex(i), [new OkCandidate()]));
        }
        return new RoundDecisionSpec(RoundDecisionPhase.Haipai, specs.ToImmutable(), null);
    }
}
