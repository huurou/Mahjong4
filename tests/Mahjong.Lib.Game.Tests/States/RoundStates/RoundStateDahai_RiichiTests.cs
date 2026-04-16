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
    public async Task 立直宣言付き打牌_OK応答で確定_持ち点1000減と供託1増()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // Act: 立直宣言付き打牌 → OK 応答 (ロンなし)
        await context_.ResponseDahaiAsync(tile, isRiichi: true);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        // Assert: 立直確定
        Assert.Equal(initialPoint - 1000, context_.Round.PointArray[dealer].Value);
        Assert.Equal(1, context_.Round.KyoutakuRiichiCount.Value);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsDoubleRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }

    [Fact]
    public async Task 立直宣言付き打牌_ロン応答で不成立_持ち点と供託は変わらない()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var initialKyoutaku = context_.Round.KyoutakuRiichiCount.Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // Act: 立直宣言付き打牌 → 子ロン
        await context_.ResponseDahaiAsync(tile, isRiichi: true);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        await RoundStateContextTestHelper.ResponseRonWinAsync(context_, new PlayerIndex(1), dealer);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);

        // Assert: 立直は不成立 → 持ち点も供託も変化なし、IsRiichi も false
        Assert.Equal(initialPoint, context_.Round.PointArray[dealer].Value);
        Assert.Equal(initialKyoutaku, context_.Round.KyoutakuRiichiCount.Value);
        Assert.False(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }

    [Fact]
    public async Task 立直宣言付き打牌_鳴き応答で立直成立_持ち点1000減と供託1増()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        var dealer = new PlayerIndex(0);
        var initialPoint = context_.Round.PointArray[dealer].Value;
        var tile = RoundStateContextTestHelper.PickTileToDahai(context_);

        // 子(P1)に Chi 用の手牌を仕込む
        var caller = new PlayerIndex(1);
        RoundStateContextTestHelper.InjectChiHand(context_, caller);

        // Act: 立直宣言付き打牌 → 子チー (鳴かれても立直成立)
        await context_.ResponseDahaiAsync(tile, isRiichi: true);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(context_);
        // RoundStateCall 経由で再度 RoundStateDahai に戻る遷移を待つ
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sawCall = false;
        context_.RoundStateChanged += (_, e) =>
        {
            if (e.State is RoundStateCall) { sawCall = true; }
            else if (sawCall && e.State is RoundStateDahai) { tcs.TrySetResult(); }
        };
        await context_.ResponseCallAsync(caller, CallType.Chi, RoundStateContextTestHelper.DummyChiHandTiles(), tile);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert: 立直成立
        Assert.Equal(initialPoint - 1000, context_.Round.PointArray[dealer].Value);
        Assert.Equal(1, context_.Round.KyoutakuRiichiCount.Value);
        Assert.True(context_.Round.PlayerRoundStatusArray[dealer].IsRiichi);
        Assert.Null(context_.Round.PendingRiichiPlayerIndex);
    }
}
