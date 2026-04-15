using Mahjong.Lib.Game.Calls;

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
        Transit(context, new RoundStateDahai(), () => context.Round = context.Round.Dahai(evt.Tile));
    }

    public override void ResponseKan(RoundStateContext context, RoundEventResponseKan evt)
    {
        base.ResponseKan(context, evt);
        Transit(
            context,
            new RoundStateKan(),
            () => context.Round = evt.CallType switch
            {
                CallType.Ankan => context.Round.Ankan(evt.Tile),
                CallType.Kakan => context.Round.Kakan(evt.Tile),
                _ => throw new InvalidOperationException($"槓応答の副露種別は Ankan / Kakan のいずれかである必要があります。実際:{evt.CallType}")
            }
        );
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
