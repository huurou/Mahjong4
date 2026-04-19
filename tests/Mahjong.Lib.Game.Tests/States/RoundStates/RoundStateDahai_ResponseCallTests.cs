using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateDahai_ResponseCallTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 副露応答_副露状態を経由してOK応答後に打牌状態へ遷移する()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseDahaiAsync(RoundStateContextTestHelper.PickTileToDahai(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);

        var passedTypes = new List<Type>();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        context_.RoundStateChanged += (_, e) =>
        {
            passedTypes.Add(e.State.GetType());
            if (passedTypes.Contains(typeof(RoundStateCall)) && e.State is RoundStateDahai)
            {
                tcs.TrySetResult();
            }
        };

        // Act
        var caller = context_.Round.Turn.Next();
        var calledTile = context_.Round.RiverArray[context_.Round.Turn].Last();
        RoundStateContextTestHelper.InjectChiHand(context_, caller);
        await context_.ResponseCallAsync(caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), calledTile);
        // RoundStateCall 遷移 → OK 応答で RoundStateDahai へ
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateCall>(context_);
        await context_.ResponseOkAsync();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(typeof(RoundStateCall), passedTypes);
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
