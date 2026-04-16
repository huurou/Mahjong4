using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateDahai_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ロン和了応答_和了状態に遷移する()
    {
        // Arrange: 親が打牌 → RoundStateDahai
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Act: 子(index 1)がロン
        await RoundStateContextTestHelper.ResponseRonWinAsync(context_, new PlayerIndex(1), new PlayerIndex(0));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task ツモ和了応答_例外で和了状態に遷移しない()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Act: 打牌状態に対して Tsumo (不正) を渡す
        await context_.ResponseWinAsync([new PlayerIndex(0)], new PlayerIndex(0), WinType.Tsumo);

        // Assert: 例外で遷移しないため Dahai のまま
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
