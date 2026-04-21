using Mahjong.Lib.Game.Tests.Games;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateContext_ConstructorTests
{
    [Fact]
    public void 正常な引数_インスタンスが生成される()
    {
        // Arrange & Act
        using var ctx = GamesTestHelper.CreateContext();

        // Assert
        Assert.NotNull(ctx);
    }
}
