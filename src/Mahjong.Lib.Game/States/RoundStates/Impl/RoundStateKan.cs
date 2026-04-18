using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓（暗槓・加槓）
/// ResponseWin は加槓に対する槍槓ロンで発生する。
/// 暗槓に対する槍槓は国士無双のみ許容されるが、Phase 5 残作業 (GameRules.AllowAnkanChankanForKokushi) 実装までは拒否する
/// </summary>
/// <param name="KanType">直前に実行された槓の種別 (Ankan または Kakan)</param>
/// <param name="KanTiles">直前の槓で使われた槓子4枚
/// (暗槓: 手牌から引き抜いた4枚 / 加槓: 元ポン3枚 + 追加牌1枚)。槍槓候補算出、将来の国士暗槓チャンカン判定に使用する</param>
public record RoundStateKan(CallType KanType, ImmutableArray<Tile> KanTiles) : RoundState
{
    public override string Name => "槓";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        Transit(context, new RoundStateKanTsumo(), () => context.Round = context.Round.RinshanTsumo());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        if (evt.WinType is not WinType.Chankan)
        {
            throw new InvalidOperationException($"槓状態からの和了応答は Chankan のみ。実際:{evt.WinType}");
        }

        // 槍槓は加槓に対してのみ発生する。暗槓は対象外 (国士無双暗槓チャンカンは Phase 5 残作業で別途対応)
        if (KanType != CallType.Kakan)
        {
            throw new InvalidOperationException($"槍槓は加槓に対してのみ成立します。直前の槓種別:{KanType}");
        }

        // 放銃者は現手番 (= 加槓宣言者)
        var loserIndex = context.Round.Turn;
        var settledRound = context.Round.SettleWin(evt.WinnerIndices, loserIndex, evt.WinType, context.ScoreCalculator);
        var eventArgs = new RoundEndedByWinEventArgs(evt.WinnerIndices, loserIndex, evt.WinType);
        Transit(context, new RoundStateWin(eventArgs), () => context.Round = settledRound);
    }

    public override RoundDecisionSpec CreateDecisionSpec(Round round, IResponseCandidateEnumerator enumerator)
    {
        if (KanType is not CallType.Ankan and not CallType.Kakan)
        {
            throw new InvalidOperationException($"RoundStateKan は暗槓/加槓でのみ有効です。実際:{KanType}");
        }

        var specs = ImmutableList.CreateBuilder<PlayerDecisionSpec>();
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (playerIndex == round.Turn) { continue; }

            specs.Add(new PlayerDecisionSpec(playerIndex, enumerator.EnumerateForKan(round, playerIndex, KanTiles, KanType)));
        }
        return new RoundDecisionSpec(RoundDecisionPhase.Kan, specs.ToImmutable(), round.Turn);
    }
}
