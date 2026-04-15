using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Games;

public class Game_CreateRoundTests
{
    [Fact]
    public void 初期Game_東一局親0のRoundが作成される()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        var wallGenerator = GamesTestHelper.CreateWallGenerator();

        // Act
        var round = game.CreateRound(wallGenerator);

        // Assert
        Assert.Equal(RoundWind.East, round.RoundWind);
        Assert.Equal(0, round.RoundNumber.Value);
        Assert.Equal(0, round.Honba.Value);
        Assert.Equal(0, round.Turn.Value);
    }

    [Fact]
    public void 南二局のGame_親1のRoundが作成される()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules())
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東二局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東三局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東四局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 南一局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);   // 南二局
        var wallGenerator = GamesTestHelper.CreateWallGenerator();

        // Act
        var round = game.CreateRound(wallGenerator);

        // Assert
        Assert.Equal(RoundWind.South, round.RoundWind);
        Assert.Equal(1, round.RoundNumber.Value);
        Assert.Equal(1, round.Turn.Value);
    }
}
