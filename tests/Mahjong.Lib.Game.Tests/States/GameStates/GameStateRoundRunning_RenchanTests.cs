using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tests.Games;
using Mahjong.Lib.Game.Tests.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateRoundRunning_RenchanTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator());

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
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await roundStateContext.ResponseWinAsync();

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(1, context_.Game.RoundNumber.Value);   // 親流れで東二局
        Assert.Equal(0, context_.Game.Honba.Value);         // 通常の親流れで本場リセット
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }

    [Fact]
    public async Task RenchanAgariOnly_親ツモ和了で連荘しHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, RenchanCondition = RenchanCondition.AgariOnly };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await roundStateContext.ResponseWinAsync();

        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(0, context_.Game.RoundNumber.Value);   // 連荘で東一局のまま
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }
}
