using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

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
        if (evt.IsRiichi)
        {
            context.Tracer.OnRiichiDeclared(context.Round.Turn, step: 1);
        }
        Transit(context, () => new RoundStateDahai(), round =>
        {
            // PendRiichi と Dahai を単一関数で適用することで、Dahai 例外時に Round が部分更新されないことを保証する
            if (evt.IsRiichi)
            {
                round = round.PendRiichi(round.Turn);
            }
            return round.Dahai(evt.Tile);
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
        var caller = context.Round.Turn;
        Transit(
            context,
            () => new RoundStateKan(evt.CallType, kanTiles),
            round => evt.CallType switch
            {
                CallType.Ankan => round.Ankan(evt.Tile),
                CallType.Kakan => round.Kakan(evt.Tile),
                _ => throw new InvalidOperationException($"槓応答の副露種別は Ankan / Kakan のいずれかである必要があります。実際:{evt.CallType}")
            }
        );
        context.Tracer.OnCallExecuted(caller, GetExecutedKanCall(context.Round, caller, evt.CallType, evt.Tile));
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            var candidates = playerIndex == round.Turn
                ? enumerator.EnumerateForAfterKanTsumo(round, playerIndex)
                : new CandidateList([new OkCandidate()]);
            specs.Add(new PlayerInquirySpec(playerIndex, candidates));
        }
        return new RoundInquirySpec(RoundInquiryPhase.AfterKanTsumo, specs.ToImmutable(), [round.Turn], round.Turn);
    }
}
