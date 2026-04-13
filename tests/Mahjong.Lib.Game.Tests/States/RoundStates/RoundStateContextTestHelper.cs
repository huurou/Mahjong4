using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

internal static class RoundStateContextTestHelper
{
    internal static async Task WaitForStateAsync<T>(
        RoundStateContext context,
        TimeSpan? timeout = null
    ) where T : RoundState
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? sender, RoundStateChangedEventArgs e)
        {
            if (e.State is T)
            {
                tcs.TrySetResult();
            }
        }

        context.RoundStateChanged += Handler;
        try
        {
            if (context.State is T)
            {
                return;
            }

            await tcs.Task.WaitAsync(timeout.Value);
        }
        finally
        {
            context.RoundStateChanged -= Handler;
        }
    }
}
