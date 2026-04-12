using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_IntegrationTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 配牌からツモ和了までの遷移パス()
    {
        // Arrange
        context_.Init();
        Assert.IsType<RoundStateHaipai>(context_.State);

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseWinAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 配牌から打牌ロン和了までの遷移パス()
    {
        // Arrange
        context_.Init();

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        await context_.ResponseWinAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 配牌からツモ打牌の一巡の遷移パス()
    {
        // Arrange
        context_.Init();

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Assert
        Assert.IsType<RoundStateTsumo>(context_.State);
    }

    [Fact]
    public async Task 副露からの打牌の遷移パス()
    {
        // Arrange
        context_.Init();
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        context_.RoundStateChanged += (_, e) =>
        {
            if (e.State is RoundStateDahai)
            {
                tcs.TrySetResult();
            }
        };

        // Act
        await context_.ResponseCallAsync();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }

    [Fact]
    public async Task 暗槓からの嶺上ツモ後打牌の遷移パス()
    {
        // Arrange
        context_.Init();
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Act
        await context_.ResponseKanAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKanTsumo>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateAfterKanTsumo>(context_);
        await context_.ResponseDahaiAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
