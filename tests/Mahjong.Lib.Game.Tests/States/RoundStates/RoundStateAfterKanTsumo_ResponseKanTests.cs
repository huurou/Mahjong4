using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateAfterKanTsumo_ResponseKanTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 槓応答_槓状態に遷移する()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Act
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));

        // Assert
        Assert.IsType<RoundStateKan>(context_.State);
    }
}
