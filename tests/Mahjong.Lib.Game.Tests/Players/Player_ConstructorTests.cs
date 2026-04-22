using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class Player_ConstructorTests
{
    [Fact]
    public void 全フィールドが保持される()
    {
        // Arrange
        var id = PlayerId.NewId();
        var index = new PlayerIndex(2);

        // Act
        var player = new PlayersTestHelper.TestPlayer(id, "A", index);

        // Assert
        Assert.Equal(id, player.PlayerId);
        Assert.Equal("A", player.DisplayName);
        Assert.Equal(index, player.PlayerIndex);
    }
}
