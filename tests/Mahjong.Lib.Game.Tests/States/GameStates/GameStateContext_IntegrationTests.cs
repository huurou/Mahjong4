using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tests.Games;
using Mahjong.Lib.Game.Tests.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateContext_IntegrationTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator());

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SingleRound_流局でGameEndに到達する()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.SingleRound };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await roundStateContext.ResponseRyuukyokuAsync();
        await GameStateContextTestHelper.WaitForStateAsync<GameStateEnd>(context_);

        // Assert
        Assert.IsType<GameStateEnd>(context_.State);
    }

    [Fact]
    public async Task Tonpuu_4局消化でGameEndに到達する()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        RoundStateContext? previousRoundStateContext = null;
        for (var i = 0; i < 4; i++)
        {
            var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previousRoundStateContext);
            previousRoundStateContext = roundStateContext;
            await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
            await roundStateContext.ResponseOkAsync();
            await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
            await roundStateContext.ResponseRyuukyokuAsync();
        }
        await GameStateContextTestHelper.WaitForStateAsync<GameStateEnd>(context_);

        // Assert
        Assert.IsType<GameStateEnd>(context_.State);
    }

    [Fact]
    public async Task 親ツモ和了で連荘しHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        // 親ツモ和了 (Turn=PlayerIndex(0) のまま ResponseWinAsync)
        await roundStateContext.ResponseWinAsync();

        // 次局開始まで待つ (2回目の RoundStateContext が生成されるまで)
        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.Equal(0, context_.Game.RoundNumber.Value);    // 連荘で東一局のまま
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }
}
