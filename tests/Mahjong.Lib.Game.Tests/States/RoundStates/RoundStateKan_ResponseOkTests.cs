using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKan_ResponseOkTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void OK応答_槓ツモ状態に遷移する()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.IsType<RoundStateKanTsumo>(context_.State);
    }
}
