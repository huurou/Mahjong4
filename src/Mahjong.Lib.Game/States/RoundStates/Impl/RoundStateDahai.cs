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
        Transit(context, new RoundStateCall());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        if (IsRyuukyoku())
        {
            Transit(context, new RoundStateRyuukyoku());
        }
        else
        {
            Transit(context, new RoundStateTsumo());
        }
    }

    private bool IsRyuukyoku()
    {
        // TODO: 流局判定（荒牌平局、四家立直、三家和了、四風連打など）
        return false;
    }
}
