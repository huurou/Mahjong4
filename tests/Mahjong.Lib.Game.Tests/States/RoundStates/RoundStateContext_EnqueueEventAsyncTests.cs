using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_EnqueueEventAsyncTests
{
    [Fact]
    public async Task Dispose後にイベント発行_ObjectDisposedExceptionが発生する()
    {
        // Arrange
        var context = new RoundStateContext();
        context.Init();
        context.Dispose();

        // Act
        var ex = await Record.ExceptionAsync(() => context.ResponseOkAsync());

        // Assert
        Assert.IsType<ObjectDisposedException>(ex);
    }
}
