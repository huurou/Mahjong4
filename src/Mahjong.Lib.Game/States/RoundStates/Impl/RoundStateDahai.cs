using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
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
        // 同巡フリテン適用は本イベント到達前に RoundManager が context.ApplyTemporaryFuriten で反映済
        var confirmedRound = context.Round.ConfirmRiichi();
        if (IsRyuukyoku(confirmedRound))
        {
            // 荒牌平局: テンパイ者・流し満貫者を判定し RyuukyokuSettler に渡す
            var tenpaiPlayers = EnumerateTenpaiPlayers(confirmedRound, context.TenpaiChecker);
            var nagashiManganPlayers = EnumerateNagashiManganPlayers(confirmedRound);
            var eventArgs = new RoundEndedByRyuukyokuEventArgs(RyuukyokuType.KouhaiHeikyoku, tenpaiPlayers, nagashiManganPlayers);
            var settledRound = confirmedRound.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, tenpaiPlayers, nagashiManganPlayers);
            Transit(context, new RoundStateRyuukyoku(eventArgs), _ => settledRound);
        }
        else
        {
            Transit(context, new RoundStateTsumo(), _ => confirmedRound.NextTurn().Tsumo());
        }
    }

    public override void ResponseCall(RoundStateContext context, RoundEventResponseCall evt)
    {
        base.ResponseCall(context, evt);

        // 副露処理は遷移時 Round 更新関数内で行い、他の Response* との一貫性を保つ。
        // SnapshotRound には副露直後の Round (= updateRound 後の Round) を封入したいため、nextStateFactory 版 Transit を使用する。
        // 大明槓時の後続 RinshanTsumo は RoundStateCall の ResponseOk 受信後に行う
        Transit(
            context,
            () => new RoundStateCall { SnapshotRound = context.Round },
            round =>
            {
                // 鳴かれても立直は成立する (リーチ棒は供託される)
                // 同巡フリテン適用は本イベント到達前に RoundManager が context.ApplyTemporaryFuriten で反映済
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
        var (settledRound, details) = round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, winTile, context.ScoreCalculator);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType, details.Winners, details.Honba, details.KyoutakuRiichiAward);
        Transit(context, new RoundStateWin(eventArgs), _ => settledRound);
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

    private static ImmutableArray<PlayerIndex> EnumerateTenpaiPlayers(Round round, ITenpaiChecker tenpaiChecker)
    {
        var builder = ImmutableArray.CreateBuilder<PlayerIndex>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (tenpaiChecker.IsTenpai(round.HandArray[playerIndex], round.CallListArray[playerIndex]))
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

    private static bool IsRyuukyoku(Round round)
    {
        // TODO: 流局判定の完全実装 (現状は荒牌平局のみ対応)
        //  - 四家立直
        //  - 三家和了
        //  - 四風連打
        //  - 四槓流れ
        //  - 九種九牌
        return round.Wall.LiveRemaining == 0;
    }
}
