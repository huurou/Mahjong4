namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓ツモ後
/// </summary>
public record RoundStateAfterKanTsumo : RoundState
{
    public override string Name => "槓ツモ後";

    public override void ResponseKan(RoundStateContext context, RoundEventResponseKan evt)
    {
        base.ResponseKan(context, evt);
        Transit(context, new RoundStateKan());
    }

    public override void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt)
    {
        base.ResponseDahai(context, evt);
        Transit(context, new RoundStateDahai());
    }
}
