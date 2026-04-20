using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

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
        Transit(context, () => new RoundStateDahai(), round =>
        {
            // 立直宣言は保留のみ。持ち点・供託は ResponseOk (ロン応答なし) または鳴き発生時に確定
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
        // Tsumo の和了牌 = 和了者の手牌末尾 (直前にツモった牌)
        var winTile = context.Round.HandArray[context.Round.Turn].Last();
        var scoreResults = CalculateScoreResults(context, context.Round, evt.WinnerIndices, loserIndex, evt.WinType, winTile);
        var (settledRound, details) = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, winTile, scoreResults);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType, details.Winners, details.Honba, details.KyoutakuRiichiAward);
        Transit(context, () => new RoundStateWin(eventArgs), _ => settledRound);
    }

    public override void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt)
    {
        base.ResponseRyuukyoku(context, evt);
        // 途中流局 (九種九牌・四風連打・四槓流れ・四家立直・三家和了) 流し満貫は荒牌平局のみなので空
        var settledRound = context.Round.SettleRyuukyoku(evt.Type, evt.TenpaiPlayers, []);
        var eventArgs = new RoundEndedByRyuukyokuEventArgs(evt.Type, evt.TenpaiPlayers, []);
        Transit(context, () => new RoundStateRyuukyoku(eventArgs), _ => settledRound);
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            var candidates = playerIndex == round.Turn
                ? enumerator.EnumerateForTsumo(round, playerIndex)
                : new CandidateList([new OkCandidate()]);
            specs.Add(new PlayerInquirySpec(playerIndex, candidates));
        }
        return new RoundInquirySpec(RoundInquiryPhase.Tsumo, specs.ToImmutable(), [round.Turn], round.Turn);
    }
}
