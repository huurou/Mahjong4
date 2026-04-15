using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKanTsumo_ResponseOkTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task OK応答_四槓流れでない場合_槓ツモ後状態に遷移する()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKanTsumo>(context_);

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateAfterKanTsumo>(context_);

        // Assert
        Assert.IsType<RoundStateAfterKanTsumo>(context_.State);
    }
}
