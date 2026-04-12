namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓（暗槓・加槓）
/// </summary>
public record RoundStateKan : RoundState
{
    public override string Name => "槓";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        Transit(context, new RoundStateKanTsumo());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }
}
