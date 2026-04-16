using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Games;

public class Game_ApplyRoundResultTests
{
    [Fact]
    public void 終了Roundの持ち点が反映される()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        var round = game.CreateRound(
            GamesTestHelper.CreateWallGenerator()
        ) with
        {
            PointArray = new PointArray(new Point(30000)).AddPoint(new PlayerIndex(0), 5000),
        };

        // Act
        var updated = game.ApplyRoundResult(round);

        // Assert
        Assert.Equal(35000, updated.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(30000, updated.PointArray[new PlayerIndex(1)].Value);
    }

    [Fact]
    public void 終了Roundの供託が反映される()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        var round = game.CreateRound(
            GamesTestHelper.CreateWallGenerator()
        ) with
        {
            KyoutakuRiichiCount = new KyoutakuRiichiCount(2),
        };

        // Act
        var updated = game.ApplyRoundResult(round);

        // Assert
        Assert.Equal(2, updated.KyoutakuRiichiCount.Value);
    }
}
