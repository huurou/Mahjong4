using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tenpai;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 打牌
/// </summary>
public record RoundStateDahai : RoundState
{
    public override string Name => "打牌";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        // ロン応答なし 保留中の立直宣言があれば確定 (持ち点-1000・供託+1)
        // 同巡フリテン適用は本イベント到達前に RoundStateContext が ApplyTemporaryFuriten で反映済
        var pendingRiichi = context.Round.PendingRiichiPlayerIndex;
        var confirmedRound = context.Round.ConfirmRiichi();
        if (pendingRiichi is { } riichiPlayer)
        {
            context.Tracer.OnRiichiDeclared(riichiPlayer, step: 2);
        }
        if (DetermineRyuukyokuType(confirmedRound) is { } type)
        {
            var isKouhai = type == RyuukyokuType.KouhaiHeikyoku;
            var tenpaiPlayers = isKouhai ? EnumerateTenpaiPlayers(confirmedRound) : [];
            var nagashiManganPlayers = isKouhai ? EnumerateNagashiManganPlayers(confirmedRound) : [];
            var (settledRound, pointDeltas) = confirmedRound.SettleRyuukyoku(type, tenpaiPlayers, nagashiManganPlayers);
            var eventArgs = new RoundEndedByRyuukyokuEventArgs(type, tenpaiPlayers, nagashiManganPlayers, pointDeltas);
            Transit(context, () => new RoundStateRyuukyoku(eventArgs), _ => settledRound);
        }
        else
        {
            Transit(context, () => new RoundStateTsumo(), _ => confirmedRound.NextTurn().Tsumo());
        }
    }

    public override void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt)
    {
        base.ResponseRyuukyoku(context, evt);
        // 三家和了: 3人ロンで流局扱い。保留中の立直はロン扱いで不成立 (リーチ棒を出さない)
        var round = context.Round.CancelRiichi();
        var (settledRound, pointDeltas) = round.SettleRyuukyoku(evt.Type, evt.TenpaiPlayers, []);
        var eventArgs = new RoundEndedByRyuukyokuEventArgs(evt.Type, evt.TenpaiPlayers, [], pointDeltas);
        Transit(context, () => new RoundStateRyuukyoku(eventArgs), _ => settledRound);
    }

    public override void ResponseCall(RoundStateContext context, RoundEventResponseCall evt)
    {
        base.ResponseCall(context, evt);

        // 鳴かれても立直は成立する。確定前に step=2 を観測通知 (updateRound 内では副作用を避けるため)
        if (context.Round.PendingRiichiPlayerIndex is { } riichiPlayer)
        {
            context.Tracer.OnRiichiDeclared(riichiPlayer, step: 2);
        }

        // 副露処理は遷移時 updateRound 内で行い、他の Response* との一貫性を保つ。
        // SnapshotRound には副露直後の Round (= updateRound 後の Round) を封入したいため、nextStateFactory 版 Transit を使用する。
        // 大明槓時の後続 RinshanTsumo は RoundStateCall の ResponseOk 受信後に行う
        Transit(
            context,
            () => new RoundStateCall { SnapshotRound = context.Round },
            round =>
            {
                // 鳴かれても立直は成立する (リーチ棒は供託される)
                // 同巡フリテン適用は本イベント到達前に RoundStateContext が ApplyTemporaryFuriten で反映済
                var preparedRound = round.ConfirmRiichi();
                return evt.CallType switch
                {
                    CallType.Chi => preparedRound.Chi(evt.Caller, evt.HandTiles),
                    CallType.Pon => preparedRound.Pon(evt.Caller, evt.HandTiles),
                    CallType.Daiminkan => preparedRound.Daiminkan(evt.Caller, evt.HandTiles),
                    _ => throw new InvalidOperationException($"副露応答の副露種別は Chi / Pon / Daiminkan のいずれかである必要があります。実際:{evt.CallType}")
                };
            }
        );
        context.Tracer.OnCallExecuted(evt.Caller, context.Round.CallListArray[evt.Caller].Last());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        if (evt.WinType is not WinType.Ron)
        {
            throw new InvalidOperationException($"打牌状態からの和了応答は Ron のみ。実際:{evt.WinType}");
        }

        // ロンされたので保留中の立直は不成立 (リーチ棒は出さない、持ち点も減らない)
        var round = context.Round.CancelRiichi();
        // 放銃者は現手番 (= 直前の打牌者)
        var loserIndex = round.Turn;
        // Ron の和了牌 = 放銃者の河末尾 (= 直前に打たれた牌)
        var winTile = round.RiverArray[loserIndex].Last();
        var scoreResults = CalculateScoreResults(context, round, evt.WinnerIndices, loserIndex, evt.WinType, winTile);
        var (settledRound, details) = round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, winTile, scoreResults);
        var eventArgs = new RoundEndedByWinEventArgs(
            evt.WinnerIndices,
            loserIndex,
            evt.WinType,
            details.Winners,
            details.Honba,
            details.KyoutakuRiichiAward,
            details.UraDoraIndicators
        );
        Transit(context, () => new RoundStateWin(eventArgs), _ => settledRound);
    }

    public override RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        var discardedTile = round.RiverArray[round.Turn].Last();
        var specs = ImmutableList.CreateBuilder<PlayerInquirySpec>();
        var inquiredBuilder = ImmutableArray.CreateBuilder<PlayerIndex>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (playerIndex == round.Turn)
            {
                specs.Add(new PlayerInquirySpec(playerIndex, new CandidateList([new OkCandidate()])));
            }
            else
            {
                specs.Add(new PlayerInquirySpec(playerIndex, enumerator.EnumerateForDahai(round, playerIndex, discardedTile)));
                inquiredBuilder.Add(playerIndex);
            }
        }
        return new RoundInquirySpec(RoundInquiryPhase.Dahai, specs.ToImmutable(), inquiredBuilder.ToImmutable(), round.Turn);
    }

    private static ImmutableArray<PlayerIndex> EnumerateTenpaiPlayers(Round round)
    {
        var builder = ImmutableArray.CreateBuilder<PlayerIndex>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (TenpaiHelper.IsTenpai(round.HandArray[playerIndex]))
            {
                builder.Add(playerIndex);
            }
        }
        return builder.ToImmutable();
    }

    private static ImmutableArray<PlayerIndex> EnumerateNagashiManganPlayers(Round round)
    {
        var builder = ImmutableArray.CreateBuilder<PlayerIndex>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (round.PlayerRoundStatusArray[playerIndex].IsNagashiMangan)
            {
                builder.Add(playerIndex);
            }
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// 流局種別を決定する。優先順位は Suufonrenda → SuuchaRiichi → KouhaiHeikyoku。
    /// 九種九牌は <see cref="RoundStateTsumo"/> で、三家和了は <see cref="RoundStateContext"/> の応答集約で、
    /// 四槓流れは <see cref="RoundStateKanTsumo"/> で判定される (本メソッドの守備範囲外)。
    /// </summary>
    private static RyuukyokuType? DetermineRyuukyokuType(Round round)
    {
        if (round.IsSuufonrenda())
        {
            return RyuukyokuType.Suufonrenda;
        }
        if (round.IsSuuchaRiichi())
        {
            return RyuukyokuType.SuuchaRiichi;
        }
        if (round.Wall.LiveRemaining == 0)
        {
            return RyuukyokuType.KouhaiHeikyoku;
        }

        return null;
    }
}
