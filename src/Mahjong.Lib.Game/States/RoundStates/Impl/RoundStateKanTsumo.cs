namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓ツモ
/// </summary>
public record RoundStateKanTsumo : RoundState
{
    public override string Name => "槓ツモ";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        // TODO: 四槓流れ判定 (4人目の槓で流局)
        Transit(context, new RoundStateAfterKanTsumo());
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }
}
