using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓ツモ後
/// 槓ツモ時に嶺上ツモ和了の判定が行われ、和了しなかった場合にのみこの状態に遷移する。
/// そのため、この状態では和了応答(ResponseWin)は発生しない。
/// </summary>
public record RoundStateAfterKanTsumo : RoundState
{
    public override string Name => "槓ツモ後";

    public override void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt)
    {
        base.ResponseDahai(context, evt);
        Transit(context, new RoundStateDahai(), () =>
        {
            var round = context.Round;
            if (evt.IsRiichi)
            {
                round = round.PendRiichi(round.Turn);
            }
            context.Round = round.Dahai(evt.Tile, context.TenpaiChecker);
        });
    }

    public override void ResponseKan(RoundStateContext context, RoundEventResponseKan evt)
    {
        base.ResponseKan(context, evt);
        var kanTiles = evt.CallType switch
        {
            CallType.Ankan => context.Round.ResolveAnkanTiles(evt.Tile),
            CallType.Kakan => context.Round.ResolveKakanTiles(evt.Tile),
            _ => throw new InvalidOperationException($"槓応答の副露種別は Ankan / Kakan のいずれかである必要があります。実際:{evt.CallType}")
        };
        Transit(
            context,
            new RoundStateKan(evt.CallType, kanTiles),
            () => context.Round = evt.CallType switch
            {
                CallType.Ankan => context.Round.Ankan(evt.Tile),
                CallType.Kakan => context.Round.Kakan(evt.Tile),
                _ => throw new InvalidOperationException($"槓応答の副露種別は Ankan / Kakan のいずれかである必要があります。実際:{evt.CallType}")
            }
        );
    }

    public override RoundDecisionSpec CreateDecisionSpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var spec = new PlayerDecisionSpec(round.Turn, enumerator.EnumerateForAfterKanTsumo(round, round.Turn));
        return new RoundDecisionSpec(RoundDecisionPhase.AfterKanTsumo, [spec], null);
    }
}
