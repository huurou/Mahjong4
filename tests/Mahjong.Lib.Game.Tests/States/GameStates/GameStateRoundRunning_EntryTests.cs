using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateRoundRunning_EntryTests : IDisposable
{
    private readonly GameStateContext context_ = GamesTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GameStateRoundRunning到達時_RoundStateContextが内部生成される()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        await context_.InitAsync(game, TestContext.Current.CancellationToken);

        // Act
        await context_.ResponseOkAsync();
        await GameStateContextTestHelper.WaitForStateAsync<GameStateRoundRunning>(context_);

        // Assert
        Assert.NotNull(context_.RoundStateContext);
    }
}
