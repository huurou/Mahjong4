using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局状態の基底クラス
/// </summary>
public abstract record GameState
{
    /// <summary>
    /// 状態名
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// OK応答
    /// </summary>
    public virtual Task ResponseOkAsync(GameStateContext context, GameEventResponseOk evt, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 和了による局終了通知
    /// </summary>
    public virtual Task RoundEndedByWinAsync(GameStateContext context, GameEventRoundEndedByWin evt, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 流局による局終了通知
    /// </summary>
    public virtual Task RoundEndedByRyuukyokuAsync(GameStateContext context, GameEventRoundEndedByRyuukyoku evt, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 状態入場時の処理
    /// <see cref="Notifications.GameNotification"/> の送信と ACK 収集など非同期処理を含める場合に使用する
    /// </summary>
    public virtual Task EntryAsync(GameStateContext context, CancellationToken ct = default)
    {
        context.OnStateChanged(this);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 状態退場時の処理
    /// </summary>
    public virtual void Exit(GameStateContext context)
    {
    }

    /// <summary>
    /// 指定された状態に遷移します
    /// 遷移時アクションで <see cref="Notifications.GameNotification"/> の送信や <see cref="GameStateContext.StartRound"/> を await できる
    /// </summary>
    protected static Task TransitAsync(
        GameStateContext context,
        GameState nextState,
        Func<Task>? action = null,
        CancellationToken ct = default
    )
    {
        return context.TransitAsync(nextState, action, ct);
    }
}
