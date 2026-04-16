using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_InitTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 二重呼び出し_InvalidOperationExceptionが発生する()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());

        // Act
        var ex = Record.Exception(() => context_.Init(RoundStateContextTestHelper.CreateRound()));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Dispose後に呼び出し_ObjectDisposedExceptionが発生する()
    {
        // Arrange
        context_.Dispose();

        // Act
        var ex = Record.Exception(() => context_.Init(RoundStateContextTestHelper.CreateRound()));

        // Assert
        Assert.IsType<ObjectDisposedException>(ex);
    }
}
