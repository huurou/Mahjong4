using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameManager_StartTests
{
    [Fact]
    public void StartAsync前_ContextアクセスでInvalidOperationException()
    {
        // Arrange
        using var manager = GamesTestHelper.CreateManager();

        // Act
        var exception = Record.Exception(() => manager.Context);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task StartAsync後_GameStateContextが取得できる()
    {
        // Arrange
        using var manager = GamesTestHelper.CreateManager();

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(manager.Context);
        // 通知経路が揃っているため、StartAsync 完了時点で GameStateRoundRunning へ遷移済み
        Assert.IsType<GameStateRoundRunning>(manager.Context.State);
    }

    [Fact]
    public async Task StartAsync2回目_InvalidOperationExceptionが発生する()
    {
        // Arrange
        using var manager = GamesTestHelper.CreateManager();
        await manager.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var exception = await Record.ExceptionAsync(() => manager.StartAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
