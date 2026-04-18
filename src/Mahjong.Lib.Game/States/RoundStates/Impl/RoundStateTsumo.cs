using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// ツモ
/// </summary>
public record RoundStateTsumo : RoundState
{
    public override string Name => "ツモ";

    public override void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt)
    {
        base.ResponseDahai(context, evt);
        Transit(context, new RoundStateDahai(), () =>
        {
            var round = context.Round;
            if (evt.IsRiichi)
            {
                // 立直宣言は保留のみ。持ち点・供託は ResponseOk (ロン応答なし) または鳴き発生時に確定
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

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        if (evt.WinType is not WinType.Tsumo)
        {
            throw new InvalidOperationException($"ツモ状態からの和了応答は Tsumo のみ。実際:{evt.WinType}");
        }

        // Loser は和了者自身 (= 現手番)
        var loserIndex = context.Round.Turn;
        var settledRound = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, context.ScoreCalculator);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType);
        Transit(context, new RoundStateWin(eventArgs), () => context.Round = settledRound);
    }

    public override void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt)
    {
        base.ResponseRyuukyoku(context, evt);
        // 途中流局 (九種九牌・四風連打・四槓流れ・四家立直・三家和了) 流し満貫は荒牌平局のみなので空
        var settledRound = context.Round.SettleRyuukyoku(evt.Type, evt.TenpaiPlayers, []);
        var eventArgs = new RoundEndedByRyuukyokuEventArgs(evt.Type, evt.TenpaiPlayers, []);
        Transit(context, new RoundStateRyuukyoku(eventArgs), () => context.Round = settledRound);
    }

    public override RoundDecisionSpec CreateDecisionSpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var spec = new PlayerDecisionSpec(round.Turn, enumerator.EnumerateForTsumo(round, round.Turn));
        return new RoundDecisionSpec(RoundDecisionPhase.Tsumo, [spec], null);
    }
}
