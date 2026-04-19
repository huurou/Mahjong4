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
    public void ロン和了応答_和了状態に遷移する()
    {
        // Arrange: 親が打牌 → RoundStateDahai
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        // Act: 子(index 1)がロン
        RoundStateContextTestHelper.DriveRonWin(context_, new PlayerIndex(1), new PlayerIndex(0));

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void ツモ和了応答_例外で和了状態に遷移しない()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        // Act: 打牌状態に対して Tsumo (不正) を渡す
        var ex = Record.Exception(() => RoundStateContextTestHelper.DriveResponseWin(context_, [new PlayerIndex(0)], new PlayerIndex(0), WinType.Tsumo));

        // Assert: 例外で遷移しないため Dahai のまま
        Assert.NotNull(ex);
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
