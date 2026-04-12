using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateAfterKanTsumo_ResponseDahaiTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 打牌応答_打牌状態に遷移する()
    {
        // Arrange
        context_.Init();
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKanTsumo>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateAfterKanTsumo>(context_);

        // Act
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
