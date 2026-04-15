using Mahjong.Lib.Game.Calls;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 副露
/// </summary>
public record RoundStateCall : RoundState
{
    public override string Name => "副露";

    public override void Entry(RoundStateContext context)
    {
        base.Entry(context);

        // 副露内容の更新は RoundStateDahai.ResponseCall で完了済み
        if (IsDaiminkan(context))
        {
            Transit(context, new RoundStateKanTsumo(), () => context.Round = context.Round.RinshanTsumo());
        }
        else
        {
            Transit(context, new RoundStateDahai());
        }
    }

    private static bool IsDaiminkan(RoundStateContext context)
    {
        var lastCall = context.Round.CallListArray[context.Round.Turn].LastOrDefault();
        return lastCall?.Type == CallType.Daiminkan;
    }
}
