using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Games;

public class Game_CreateTests
{
    [Fact]
    public void 初期状態_東一局0本場で作成される()
    {
        // Arrange
        var players = GamesTestHelper.CreatePlayerList();
        var rules = new GameRules();

        // Act
        var game = GameAggregate.Create(players, rules);

        // Assert
        Assert.Equal(RoundWind.East, game.RoundWind);
        Assert.Equal(0, game.RoundNumber.Value);
        Assert.Equal(0, game.Honba.Value);
        Assert.Equal(0, game.KyoutakuRiichiCount.Value);
    }

    [Fact]
    public void 初期持ち点_RulesのInitialPointsが全員に配布される()
    {
        // Arrange
        var players = GamesTestHelper.CreatePlayerList();
        var rules = new GameRules { InitialPoints = 30000 };

        // Act
        var game = GameAggregate.Create(players, rules);

        // Assert
        foreach (var i in Enumerable.Range(0, 4))
        {
            Assert.Equal(30000, game.PointArray[new PlayerIndex(i)].Value);
        }
    }
}
