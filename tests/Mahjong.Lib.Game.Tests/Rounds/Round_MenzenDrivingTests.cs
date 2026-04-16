using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_MenzenDrivingTests
{
    [Fact]
    public void Chi_callerのIsMenzenがfalseになる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);

        // Act
        var result = round.Chi(caller, ImmutableList.Create(new Tile(84), new Tile(88)));

        // Assert
        Assert.False(result.PlayerRoundStatusArray[caller].IsMenzen);
        Assert.True(result.PlayerRoundStatusArray[new PlayerIndex(0)].IsMenzen);
        Assert.True(result.PlayerRoundStatusArray[new PlayerIndex(2)].IsMenzen);
        Assert.True(result.PlayerRoundStatusArray[new PlayerIndex(3)].IsMenzen);
    }

    [Fact]
    public void 副露前_全員がIsMenzenTrue()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai();

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.True(round.PlayerRoundStatusArray[new PlayerIndex(i)].IsMenzen);
        }
    }
}
