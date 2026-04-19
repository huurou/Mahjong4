using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKan_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 暗槓中のChankan応答_受理されてRoundStateWinに遷移する()
    {
        // Arrange: 暗槓直後の RoundStateKan
        // 暗槓チャンカンは国士無双時のみ成立するが、役制限は ScoreCalculator 側に委譲するため、
        // RoundStateKan 単体では Chankan を一律受理する
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);

        // Act
        await RoundStateContextTestHelper.ResponseChankanWinAsync(context_, new PlayerIndex(1), new PlayerIndex(0));

        // Assert
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 加槓が末尾でない場合でも槍槓が成立する()
    {
        // Arrange: 親に2つのポン (kind 0, kind 1) を持たせ、最初のポン (kind 0) を加槓
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        var dealer = context_.Round.Turn;
        var pon1 = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(3)], new PlayerIndex(1), new Tile(3));
        var pon2 = new Call(CallType.Pon, [new Tile(4), new Tile(5), new Tile(7)], new PlayerIndex(1), new Tile(7));
        context_.Round = context_.Round with
        {
            HandArray = context_.Round.HandArray
                .AddTile(dealer, new Tile(2)),
            CallListArray = context_.Round.CallListArray
                .AddCall(dealer, pon1)
                .AddCall(dealer, pon2),
        };

        // Kakan on first pon (kind 0) — replaces at index 0, not the end
        await context_.ResponseKanAsync(CallType.Kakan, new Tile(2));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);

        // Act: 槍槓
        await RoundStateContextTestHelper.ResponseChankanWinAsync(context_, new PlayerIndex(1), dealer);

        // Assert: RoundStateWin に遷移
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task 過去に加槓済みで今回暗槓_Chankan応答は受理される()
    {
        // Arrange: 副露リストに過去の加槓を含むが、今回の操作は暗槓
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);

        var dealer = context_.Round.Turn;
        var pastKakan = new Call(CallType.Kakan, [new Tile(0), new Tile(1), new Tile(2), new Tile(3)], dealer, new Tile(0));
        context_.Round = context_.Round with
        {
            CallListArray = context_.Round.CallListArray.AddCall(dealer, pastKakan),
        };

        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);

        // Act
        await RoundStateContextTestHelper.ResponseChankanWinAsync(context_, new PlayerIndex(1), dealer);

        // Assert: 暗槓 Chankan も受理して RoundStateWin へ遷移
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateWin>(context_);
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public async Task ツモ和了応答_例外で遷移しない()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());
        await context_.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(context_);
        await context_.ResponseKanAsync(CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateKan>(context_);

        // Act: Tsumo を渡す (槍槓状態に Tsumo は不正)
        await RoundStateContextTestHelper.ResponseTsumoWinAsync(context_);

        // Assert
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.IsType<RoundStateKan>(context_.State);
    }
}
