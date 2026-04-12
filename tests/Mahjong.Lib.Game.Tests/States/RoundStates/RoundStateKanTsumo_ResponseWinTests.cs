using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKanTsumo_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 和了応答_和了状態に遷移する()
    {
        // Arrange
        context_.Init();
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKanTsumo>(context_);

        // Act
        await context_.ResponseWinAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }
}
