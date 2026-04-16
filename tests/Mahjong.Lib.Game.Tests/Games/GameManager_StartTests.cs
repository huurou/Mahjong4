using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameManager_StartTests
{
    [Fact]
    public void Start前_ContextアクセスでInvalidOperationException()
    {
        // Arrange
        using var manager = new GameManager(
            GamesTestHelper.CreatePlayerList(),
            new GameRules(),
            GamesTestHelper.CreateWallGenerator(),
            GamesTestHelper.CreateNoOpScoreCalculator(),
            GamesTestHelper.CreateNoOpTenpaiChecker()
        );

        // Act
        var exception = Record.Exception(() => manager.Context);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Start後_GameStateContextが取得できる()
    {
        // Arrange
        using var manager = new GameManager(
            GamesTestHelper.CreatePlayerList(),
            new GameRules(),
            GamesTestHelper.CreateWallGenerator(),
            GamesTestHelper.CreateNoOpScoreCalculator(),
            GamesTestHelper.CreateNoOpTenpaiChecker()
        );

        // Act
        manager.Start();

        // Assert
        Assert.NotNull(manager.Context);
        Assert.IsType<GameStateInit>(manager.Context.State);
    }

    [Fact]
    public void Start2回目_InvalidOperationExceptionが発生する()
    {
        // Arrange
        using var manager = new GameManager(
            GamesTestHelper.CreatePlayerList(),
            new GameRules(),
            GamesTestHelper.CreateWallGenerator(),
            GamesTestHelper.CreateNoOpScoreCalculator(),
            GamesTestHelper.CreateNoOpTenpaiChecker()
        );
        manager.Start();

        // Act
        var exception = Record.Exception(manager.Start);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
