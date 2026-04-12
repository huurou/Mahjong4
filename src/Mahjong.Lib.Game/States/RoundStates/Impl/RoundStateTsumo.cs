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
        Transit(context, new RoundStateDahai());
    }

    public override void ResponseKan(RoundStateContext context, RoundEventResponseKan evt)
    {
        base.ResponseKan(context, evt);
        Transit(context, new RoundStateKan());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }

    public override void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt)
    {
        base.ResponseRyuukyoku(context, evt);
        Transit(context, new RoundStateRyuukyoku());
    }
}
