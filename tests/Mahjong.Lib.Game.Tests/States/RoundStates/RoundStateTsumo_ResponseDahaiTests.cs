using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateTsumo_ResponseDahaiTests : IDisposable
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
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Act
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
