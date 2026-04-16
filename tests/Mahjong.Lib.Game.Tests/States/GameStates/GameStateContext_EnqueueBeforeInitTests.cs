using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateContext_EnqueueBeforeInitTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator(), GamesTestHelper.CreateNoOpScoreCalculator(), GamesTestHelper.CreateNoOpTenpaiChecker());

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Init前_ResponseOkAsyncでInvalidOperationException()
    {
        // Arrange & Act
        var exception = await Record.ExceptionAsync(context_.ResponseOkAsync);

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
