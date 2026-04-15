using Mahjong.Lib.Game.Games;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameManager_ConstructorTests
{
    [Fact]
    public void 正常な引数_インスタンスが生成される()
    {
        // Arrange & Act
        using var manager = new GameManager(
            GamesTestHelper.CreatePlayerList(),
            new GameRules(),
            GamesTestHelper.CreateWallGenerator()
        );

        // Assert
        Assert.NotNull(manager);
    }
}
