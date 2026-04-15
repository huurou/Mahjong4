namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 配牌
/// </summary>
public record RoundStateHaipai : RoundState
{
    public override string Name => "配牌";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        Transit(context, new RoundStateTsumo(), () => context.Round = context.Round.Tsumo());
    }
}
