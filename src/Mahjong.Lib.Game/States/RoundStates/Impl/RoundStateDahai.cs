using Mahjong.Lib.Game.Calls;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 打牌
/// </summary>
public record RoundStateDahai : RoundState
{
    public override string Name => "打牌";

    public override void ResponseCall(RoundStateContext context, RoundEventResponseCall evt)
    {
        base.ResponseCall(context, evt);
        Transit(
            context,
            new RoundStateCall(),
            () => context.Round = evt.CallType switch
            {
                CallType.Chi => context.Round.Chi(evt.Caller, evt.HandTiles),
                CallType.Pon => context.Round.Pon(evt.Caller, evt.HandTiles),
                CallType.Daiminkan => context.Round.Daiminkan(evt.Caller, evt.HandTiles),
                _ => throw new InvalidOperationException($"副露応答の副露種別は Chi / Pon / Daiminkan のいずれかである必要があります。実際:{evt.CallType}")
            }
        );
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        if (IsRyuukyoku(context))
        {
            Transit(context, new RoundStateRyuukyoku());
        }
        else
        {
            Transit(context, new RoundStateTsumo(), () => context.Round = context.Round.NextTurn().Tsumo());
        }
    }

    private static bool IsRyuukyoku(RoundStateContext context)
    {
        // TODO: 流局判定の完全実装 (現状は荒牌平局のみ対応)
        //  - 四家立直
        //  - 三家和了
        //  - 四風連打
        //  - 四槓流れ
        //  - 九種九牌
        return context.Round.Wall.LiveRemaining == 0;
    }
}
