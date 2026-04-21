using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateContext_StartAsyncTests
{
    [Fact]
    public void StartAsync前_StateアクセスでInvalidOperationException()
    {
        // Arrange
        using var ctx = GamesTestHelper.CreateContext();

        // Act
        var exception = Record.Exception(() => ctx.State);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task StartAsync前_ResponseOkAsyncでInvalidOperationException()
    {
        // Arrange
        using var ctx = GamesTestHelper.CreateContext();

        // Act
        var exception = await Record.ExceptionAsync(ctx.ResponseOkAsync);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task StartAsync後_GameStateRoundRunningへ遷移済み()
    {
        // Arrange
        using var ctx = GamesTestHelper.CreateContext();

        // Act
        await ctx.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        // 通知経路が揃っているため、StartAsync 完了時点で GameStateRoundRunning へ遷移済み
        Assert.IsType<GameStateRoundRunning>(ctx.State);
    }

    [Fact]
    public async Task StartAsync2回目_InvalidOperationExceptionが発生する()
    {
        // Arrange
        using var ctx = GamesTestHelper.CreateContext();
        await ctx.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var exception = await Record.ExceptionAsync(() => ctx.StartAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
