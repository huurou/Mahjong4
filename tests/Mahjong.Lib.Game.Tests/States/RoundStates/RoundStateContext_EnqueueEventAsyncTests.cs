namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_EnqueueEventAsyncTests
{
    [Fact]
    public async Task Dispose後にイベント発行_ObjectDisposedExceptionが発生する()
    {
        // Arrange
        var context = RoundStateContextTestHelper.CreateContext();
        context.Init(RoundStateContextTestHelper.CreateRound());
        context.Dispose();

        // Act
        var ex = await Record.ExceptionAsync(context.ResponseOkAsync);

        // Assert
        Assert.IsType<ObjectDisposedException>(ex);
    }

    [Fact]
    public async Task Init前にイベント発行_InvalidOperationExceptionが発生する()
    {
        // Arrange
        using var context = RoundStateContextTestHelper.CreateContext();

        // Act
        var ex = await Record.ExceptionAsync(context.ResponseOkAsync);

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }
}
