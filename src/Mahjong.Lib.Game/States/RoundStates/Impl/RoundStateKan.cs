using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓（暗槓・加槓）
/// ResponseWin は加槓に対する槍槓ロンでのみ発生する
/// </summary>
/// <param name="KanType">直前に実行された槓の種別 (Ankan または Kakan)</param>
public record RoundStateKan(CallType KanType) : RoundState
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

        // 槍槓は加槓に対してのみ発生する。暗槓は対象外 (国士チャンカンは Phase 5 で別途検討)
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
}
