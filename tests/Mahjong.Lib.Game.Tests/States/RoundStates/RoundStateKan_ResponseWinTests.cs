using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKan_ResponseWinTests : IDisposable
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
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);

        // Act
        await context_.ResponseWinAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }
}
