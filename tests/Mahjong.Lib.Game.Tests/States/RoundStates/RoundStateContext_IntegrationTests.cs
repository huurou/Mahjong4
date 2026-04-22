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
    public void 配牌からツモ和了までの遷移パス()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        Assert.IsType<RoundStateHaipai>(context_.State);

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.ReplaceHandForTsumoAgari(context_, context_.Round.Turn);
        RoundStateContextTestHelper.DriveTsumoWin(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void 配牌から打牌ロン和了までの遷移パス()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.InjectRonAgariScenario(context_, new PlayerIndex(1));
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));
        // 子(index 1)が親(index 0)からロン
        RoundStateContextTestHelper.DriveRonWin(context_, new PlayerIndex(1), new PlayerIndex(0));

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void 配牌からツモ打牌の一巡の遷移パス()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.IsType<RoundStateTsumo>(context_.State);
    }

    [Fact]
    public void 副露からの副露後状態の遷移パス()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        // Act
        var caller = context_.Round.Turn.Next();
        var calledTile = context_.Round.RiverArray[context_.Round.Turn].Last();
        RoundStateContextTestHelper.InjectChiHand(context_, caller);
        RoundStateContextTestHelper.DriveResponseCall(context_, caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), calledTile);
        // RoundStateCall は OK 応答を待つため明示的に送る
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert: 副露後は副露者に打牌を求める RoundStateAfterCall に遷移する
        Assert.IsType<RoundStateAfterCall>(context_.State);
    }

    [Fact]
    public void 暗槓からの嶺上ツモ後打牌の遷移パス()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Act
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        // Assert
        Assert.IsType<RoundStateDahai>(context_.State);
    }
}
