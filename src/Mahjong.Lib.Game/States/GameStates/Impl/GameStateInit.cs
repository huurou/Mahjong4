namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 対局開始
/// </summary>
public record GameStateInit : GameState
{
    public override string Name => "対局開始";

    public override void ResponseOk(GameStateContext context, GameEventResponseOk evt)
    {
        Transit(
            context,
            new GameStateRoundRunning(),
            () =>
            {
                var round = context.Game.CreateRound(context.WallGenerator);
                context.StartRound(round);
            }
        );
    }
}
