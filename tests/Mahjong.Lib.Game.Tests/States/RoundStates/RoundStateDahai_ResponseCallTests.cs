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
    public void 副露応答_副露状態を経由してOK応答後に打牌状態へ遷移する()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        var passedTypes = new List<Type>();
        context_.RoundStateChanged += (_, e) => passedTypes.Add(e.State.GetType());

        // Act
        var caller = context_.Round.Turn.Next();
        var calledTile = context_.Round.RiverArray[context_.Round.Turn].Last();
        RoundStateContextTestHelper.InjectChiHand(context_, caller);
        RoundStateContextTestHelper.DriveResponseCall(context_, caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), calledTile);
        // RoundStateCall 遷移 → OK 応答で RoundStateDahai へ
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.Contains(typeof(RoundStateCall), passedTypes);
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
