using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class RoundManager_NormalizeLoserIndexTests
{
    [Fact]
    public void WinTypeがTsumo_nullに正規化される()
    {
        // Arrange
        var loser = new PlayerIndex(2);

        // Act
        var result = RoundManager.NormalizeLoserIndex(loser, WinType.Tsumo);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WinTypeがRinshan_nullに正規化される()
    {
        // Arrange
        var loser = new PlayerIndex(1);

        // Act
        var result = RoundManager.NormalizeLoserIndex(loser, WinType.Rinshan);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WinTypeがRon_loserIndexがそのまま残る()
    {
        // Arrange
        var loser = new PlayerIndex(3);

        // Act
        var result = RoundManager.NormalizeLoserIndex(loser, WinType.Ron);

        // Assert
        Assert.Equal(loser, result);
    }

    [Fact]
    public void WinTypeがChankan_loserIndexがそのまま残る()
    {
        // Arrange
        var loser = new PlayerIndex(0);

        // Act
        var result = RoundManager.NormalizeLoserIndex(loser, WinType.Chankan);

        // Assert
        Assert.Equal(loser, result);
    }
}
