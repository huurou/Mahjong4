using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateTsumo_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 和了応答_和了状態に遷移する()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        // 親(Turn)の手牌を 14 枚の有効な和了形に差し替えて ScoringHelper がエラーにならないようにする
        RoundStateContextTestHelper.ReplaceHandForTsumoAgari(context_, context_.Round.Turn);

        // Act
        RoundStateContextTestHelper.DriveTsumoWin(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }
}
