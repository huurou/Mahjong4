using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tests.Games;
using Mahjong.Lib.Game.Tests.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateRoundRunning_RenchanTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator(), GamesTestHelper.CreateNoOpScoreCalculator(), GamesTestHelper.CreateNoOpTenpaiChecker());

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task RenchanNone_親ツモ和了でも次局に進む()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, RenchanCondition = RenchanCondition.None };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await RoundStateContextTestHelper.ResponseTsumoWinAsync(roundStateContext);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateWin>(roundStateContext);

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(1, context_.Game.RoundNumber.Value);   // 親流れで東二局
        Assert.Equal(0, context_.Game.Honba.Value);         // 通常の親流れで本場リセット
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }

    [Fact]
    public async Task RenchanAgariOrTenpai_親テンパイ荒牌平局で連荘しHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, RenchanCondition = RenchanCondition.AgariOrTenpai };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await roundStateContext.ResponseRyuukyokuAsync(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0)]);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateRyuukyoku>(roundStateContext);

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(0, context_.Game.RoundNumber.Value);   // 親テンパイ連荘で東一局のまま
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }

    [Fact]
    public async Task RenchanAgariOrTenpai_親ノーテン荒牌平局で親流れしHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, RenchanCondition = RenchanCondition.AgariOrTenpai };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        // 親 (index 0) はノーテン、他家1人のみテンパイ
        await roundStateContext.ResponseRyuukyokuAsync(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(1)]);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateRyuukyoku>(roundStateContext);

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert: 親流れで東二局、本場+1
        Assert.Equal(1, context_.Game.RoundNumber.Value);
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }

    [Fact]
    public async Task 途中流局_KyuushuKyuuhai_同一親で本場1増加()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await roundStateContext.ResponseRyuukyokuAsync(RyuukyokuType.KyuushuKyuuhai, []);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateRyuukyoku>(roundStateContext);

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert: 途中流局なので同一親 (東一局)、本場+1
        Assert.Equal(0, context_.Game.RoundNumber.Value);
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }

    [Fact]
    public async Task RenchanAgariOnly_親ツモ和了で連荘しHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, RenchanCondition = RenchanCondition.AgariOnly };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await RoundStateContextTestHelper.ResponseTsumoWinAsync(roundStateContext);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateWin>(roundStateContext);

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(0, context_.Game.RoundNumber.Value);   // 連荘で東一局のまま
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }
}
