using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_IntegrationTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 配牌からツモ和了までの遷移パス()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        Assert.IsType<RoundStateHaipai>(context_.State);

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await RoundStateContextTestHelper.ResponseTsumoWinAsync(context_);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 配牌から打牌ロン和了までの遷移パス()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        // 子(index 1)が親(index 0)からロン
        await RoundStateContextTestHelper.ResponseRonWinAsync(context_, new PlayerIndex(1), new PlayerIndex(0));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 配牌からツモ打牌の一巡の遷移パス()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());

        // Act
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
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
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
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
        var caller = context_.Round.Turn.Next();
        var calledTile = context_.Round.RiverArray[context_.Round.Turn].Last();
        RoundStateContextTestHelper.InjectChiHand(context_, caller);
        await context_.ResponseCallAsync(caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), calledTile);
        // RoundStateCall は OK 応答を待つため明示的に送る
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateCall>(context_);
        await context_.ResponseOkAsync();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }

    [Fact]
    public async Task 暗槓からの嶺上ツモ後打牌の遷移パス()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Act
        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKanTsumo>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateAfterKanTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
