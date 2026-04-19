using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateDahai_RiichiTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 立直宣言付き打牌_OK応答で確定_持ち点1000減と供託1増()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // Act: 立直宣言付き打牌 → OK 応答 (ロンなし)
        RoundStateContextTestHelper.DriveResponseDahai(context_, tile, isRiichi: true);
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert: 立直確定
        Assert.Equal(initialPoint - 1000, context_.Round.PointArray[dealer].Value);
        Assert.Equal(1, context_.Round.KyoutakuRiichiCount.Value);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsDoubleRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }

    [Fact]
    public void 立直宣言付き打牌_ロン応答で不成立_持ち点と供託は変わらない()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var initialKyoutaku = context_.Round.KyoutakuRiichiCount.Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // Act: 立直宣言付き打牌 → 子ロン
        RoundStateContextTestHelper.DriveResponseDahai(context_, tile, isRiichi: true);
        RoundStateContextTestHelper.DriveRonWin(context_, new PlayerIndex(1), dealer);

        // Assert: 立直は不成立 → 持ち点も供託も変化なし、IsRiichi も false
        Assert.Equal(initialPoint, context_.Round.PointArray[dealer].Value);
        Assert.Equal(initialKyoutaku, context_.Round.KyoutakuRiichiCount.Value);
        Assert.False(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }

    [Fact]
    public void 立直宣言付き打牌_鳴き応答で立直成立_持ち点1000減と供託1増()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // 子(P1)に Chi 用の手牌を仕込む
        var caller = new PlayerIndex(1);
        RoundStateContextTestHelper.InjectChiHand(context_, caller);

        // Act: 立直宣言付き打牌 → 子チー (鳴かれても立直成立)
        RoundStateContextTestHelper.DriveResponseDahai(context_, tile, isRiichi: true);
        var passedTypes = new List<Type>();
        context_.RoundStateChanged += (_, e) => passedTypes.Add(e.State.GetType());
        RoundStateContextTestHelper.DriveResponseCall(context_, caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), tile);
        // RoundStateCall 経由で再度 RoundStateDahai に戻る
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert: 立直成立
        Assert.Contains(typeof(RoundStateCall), passedTypes);
        Assert.Equal(initialPoint - 1000, context_.Round.PointArray[dealer].Value);
        Assert.Equal(1, context_.Round.KyoutakuRiichiCount.Value);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }
}
