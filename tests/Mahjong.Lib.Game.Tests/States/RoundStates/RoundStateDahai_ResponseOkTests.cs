using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateDahai_ResponseOkTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task OK応答_流局でない場合_ツモ状態に遷移する()
    {
        // Arrange
        context_.Init();
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Assert
        Assert.IsType<RoundStateTsumo>(context_.State);
    }
}
