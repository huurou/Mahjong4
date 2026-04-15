using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Games;

public class Game_AdvanceToNextRoundTests
{
    [Fact]
    public void Renchan_RoundNumber維持でHonba1増加()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());

        // Act
        var next = game.AdvanceToNextRound(RoundAdvanceMode.Renchan);

        // Assert
        Assert.Equal(RoundWind.East, next.RoundWind);
        Assert.Equal(0, next.RoundNumber.Value);
        Assert.Equal(1, next.Honba.Value);
    }

    [Fact]
    public void DealerChangeResetHonba_RoundNumber1増加でHonba0にリセット()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules())
            .AdvanceToNextRound(RoundAdvanceMode.Renchan); // 東一1本場

        // Act
        var next = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);

        // Assert
        Assert.Equal(RoundWind.East, next.RoundWind);
        Assert.Equal(1, next.RoundNumber.Value);
        Assert.Equal(0, next.Honba.Value);
    }

    [Fact]
    public void 東四局_南一局に進む()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules())
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東二局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東三局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);   // 東四局

        // Act
        var next = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);

        // Assert
        Assert.Equal(RoundWind.South, next.RoundWind);
        Assert.Equal(0, next.RoundNumber.Value);
    }

    [Fact]
    public void DealerChangeWithHonba_RoundNumber1増加かつHonba1増加()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());

        // Act
        var next = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeWithHonba);

        // Assert
        Assert.Equal(1, next.RoundNumber.Value);
        Assert.Equal(1, next.Honba.Value);
    }

    [Fact]
    public void 北四局から東一局への循環_仕様として固定()
    {
        // Arrange
        // RoundWind は東→南→西→北→東 と循環する仕様 (通常ルールでは GameEndPolicy で到達しない防御ガード)
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules());
        for (var i = 0; i < 15; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        // 北四局相当

        // Act
        var next = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);

        // Assert
        Assert.Equal(RoundWind.East, next.RoundWind);
        Assert.Equal(0, next.RoundNumber.Value);
    }
}
