using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateTsumo_ResponseRyuukyokuTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 流局応答_流局状態に遷移する()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Act
        await RoundStateContextTestHelper.ResponseKouhaiHeikyokuAsync(context_);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateRyuukyoku>(context_);

        // Assert
        Assert.IsType<RoundStateRyuukyoku>(context_.State);
    }
}
