using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓（暗槓・加槓）
/// ResponseWin は加槓に対する槍槓ロン、および暗槓に対する国士無双の槍槓ロンで発生する。
/// 暗槓チャンカンは <see cref="GameRules.AllowAnkanChankanForKokushi"/> が true かつ国士無双テンパイの場合のみ成立する。
/// 暗槓チャンカンの非国士役除外は <see cref="ResponseCandidateEnumerator.EnumerateForKan"/> の候補列挙段階で
/// 「暗槓 + 全幺九牌 + ロン候補成立」の 3 条件を確認することで保証する
/// (<see cref="ScoringHelper"/> には暗槓/加槓を区別する情報が渡されないため、ここでは候補絞り込み側に責任を置く)
/// </summary>
/// <param name="KanType">直前に実行された槓の種別 (Ankan または Kakan)</param>
/// <param name="KanTiles">直前の槓で使われた槓子4枚
/// (暗槓: 手牌から引き抜いた4枚 / 加槓: 元ポン3枚 + 追加牌1枚)。槍槓候補算出に使用する</param>
public record RoundStateKan(CallType KanType, ImmutableArray<Tile> KanTiles) : RoundState
{
    public override string Name => "槓";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        // 四槓流れは嶺上ツモ**前**に判定する (Design.md 準拠)。
        // 嶺上牌を引いた後に判定すると嶺上開花候補が誤って提示され、嶺上牌・壁が無駄に1枚消費される
        if (context.Round.IsSuukaikan())
        {
            var (settledRound, pointDeltas) = context.Round.SettleRyuukyoku(RyuukyokuType.Suukaikan, [], []);
            var eventArgs = new RoundEndedByRyuukyokuEventArgs(RyuukyokuType.Suukaikan, [], [], pointDeltas);
            Transit(context, () => new RoundStateRyuukyoku(eventArgs), _ => settledRound);
            return;
        }
        Transit(context, () => new RoundStateKanTsumo(), round => round.RinshanTsumo());
    }

    public override void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt)
    {
        base.ResponseRyuukyoku(context, evt);
        // 槍槓フェーズで三家和了 (3人チャンカンロン) が成立した場合の流局処理。
        // 加槓は Round に既に反映済。立直保留はこのフェーズで通常発生しないが CancelRiichi は保留無しなら無害
        var round = context.Round.CancelRiichi();
        var (settledRound, pointDeltas) = round.SettleRyuukyoku(evt.Type, evt.TenpaiPlayers, []);
        var eventArgs = new RoundEndedByRyuukyokuEventArgs(evt.Type, evt.TenpaiPlayers, [], pointDeltas);
        Transit(context, () => new RoundStateRyuukyoku(eventArgs), _ => settledRound);
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        if (evt.WinType is not WinType.Chankan)
        {
            throw new InvalidOperationException($"槓状態からの和了応答は Chankan のみ。実際:{evt.WinType}");
        }

        // 放銃者は現手番 (= 槓宣言者)。
        // 暗槓チャンカンは国士無双のみ成立可能 (候補絞り込みは ResponseCandidateEnumerator で保証済み)。
        var loserIndex = context.Round.Turn;
        // Chankan の和了牌 = 槓で追加された牌 (加槓: addedTile / 暗槓: 国士対応時の槓子末尾)。
        // 副露リスト末尾ではなく KanTiles 末尾を参照するため、過去の加槓/ポン等と混同しない
        var winTile = KanTiles[^1];
        var scoreResults = CalculateScoreResults(context, context.Round, evt.WinnerIndices, loserIndex, evt.WinType, winTile);
        var (settledRound, details) = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, winTile, scoreResults);
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
        if (KanType is not CallType.Ankan and not CallType.Kakan)
        {
            throw new InvalidOperationException($"RoundStateKan は暗槓/加槓でのみ有効です。実際:{KanType}");
        }

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
                specs.Add(new PlayerInquirySpec(playerIndex, enumerator.EnumerateForKan(round, playerIndex, KanTiles, KanType)));
                inquiredBuilder.Add(playerIndex);
            }
        }
        return new RoundInquirySpec(RoundInquiryPhase.Kan, specs.ToImmutable(), inquiredBuilder.ToImmutable(), round.Turn);
    }
}
