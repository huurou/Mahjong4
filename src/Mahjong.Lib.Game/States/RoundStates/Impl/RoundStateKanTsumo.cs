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
        if (IsSuukantsuNagare())
        {
            Transit(context, new RoundStateRyuukyoku());
        }
        else
        {
            Transit(context, new RoundStateAfterKanTsumo());
        }
    }

    public override void ResponseWin(RoundStateContext context, RoundEventResponseWin evt)
    {
        base.ResponseWin(context, evt);
        Transit(context, new RoundStateWin());
    }

    private bool IsSuukantsuNagare()
    {
        // TODO: 四槓流れ判定
        return false;
    }
}
