namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 対局開始
/// 通知・応答集約経路では <see cref="GameStateContext.InitAsync"/> 自身が初期通知送信と次状態遷移を担う
/// (同期経路の場合のみ <see cref="ResponseOkAsync"/> で <see cref="GameStateRoundRunning"/> へ遷移する)
/// </summary>
public record GameStateInit : GameState
{
    public override string Name => "対局開始";

    public override Task ResponseOkAsync(GameStateContext context, GameEventResponseOk evt, CancellationToken ct = default)
    {
        return TransitAsync(
            context,
            new GameStateRoundRunning(),
            () =>
            {
                var round = context.Game.CreateRound(context.WallGenerator);
                context.StartRound(round);
                return Task.CompletedTask;
            },
            ct
        );
    }
}
