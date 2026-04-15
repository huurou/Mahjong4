using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateInit_ResponseOkTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator());

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ResponseOk_GameStateRoundRunningに遷移する()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        context_.Init(game);
        Assert.IsType<GameStateInit>(context_.State);

        // Act
        await context_.ResponseOkAsync();
        await GameStateContextTestHelper.WaitForStateAsync<GameStateRoundRunning>(context_);

        // Assert
        Assert.IsType<GameStateRoundRunning>(context_.State);
    }
}
