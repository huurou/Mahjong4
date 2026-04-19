using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateInit_ResponseOkTests : IDisposable
{
    private readonly GameStateContext context_ = GamesTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task InitAsync後_GameStartNotification送信を経てGameStateRoundRunningに到達する()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());

        // Act: RoundManager 経路では InitAsync が GameStart 通知送信 → GameStateRoundRunning 遷移まで進める
        await context_.InitAsync(game, TestContext.Current.CancellationToken);
        await GameStateContextTestHelper.WaitForStateAsync<GameStateRoundRunning>(context_);

        // Assert
        Assert.IsType<GameStateRoundRunning>(context_.State);
    }
}
