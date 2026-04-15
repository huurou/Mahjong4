using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

internal static class GameStateContextTestHelper
{
    internal static async Task WaitForStateAsync<T>(
        GameStateContext context,
        TimeSpan? timeout = null
    ) where T : GameState
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is T)
            {
                tcs.TrySetResult();
            }
        }

        context.GameStateChanged += Handler;
        try
        {
            if (context.State is not T)
            {
                await tcs.Task.WaitAsync(timeout.Value);
            }
        }
        finally
        {
            context.GameStateChanged -= Handler;
        }
    }

    /// <summary>
    /// CurrentRoundContext が previous と異なる新しいインスタンスになるまで待機します
    /// 複数局を跨ぐテストで 局のRoundStateContext が入れ替わるのを確実に待つために使用
    /// </summary>
    internal static async Task<RoundStateContext> WaitForNewRoundContextAsync(
        GameStateContext context,
        RoundStateContext? previous,
        TimeSpan? timeout = null
    )
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var deadline = DateTime.UtcNow + timeout.Value;
        while (DateTime.UtcNow < deadline)
        {
            if (context.RoundStateContext is { } roundStateContext && !ReferenceEquals(roundStateContext, previous))
            {
                return roundStateContext;
            }

            await Task.Delay(10);
        }
        throw new TimeoutException("新しい RoundStateContext が生成されませんでした。");
    }
}
