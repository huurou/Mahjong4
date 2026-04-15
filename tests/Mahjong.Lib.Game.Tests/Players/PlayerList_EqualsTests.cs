using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class PlayerList_EqualsTests
{
    [Fact]
    public void 同じプレイヤー列の別インスタンス_等価になる()
    {
        // Arrange
        var players = PlayersTestHelper.CreateTestPlayers(4);
        var a = new PlayerList(players);
        var b = new PlayerList(players);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なるプレイヤー列_非等価になる()
    {
        // Arrange
        var a = new PlayerList(PlayersTestHelper.CreateTestPlayers(4));
        var b = new PlayerList(PlayersTestHelper.CreateTestPlayers(4));

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
