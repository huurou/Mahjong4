using Mahjong.Lib.Game.States.GameStates;
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
        // Arrange & Act: StartAsync 完了時点で GameStateRoundRunning へ遷移済みで、RoundStateContext も生成済み
        await context_.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(context_.RoundStateContext);
    }
}
