using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class PlayerList_ConstructorTests
{
    [Fact]
    public void プレイヤー4人_正常に作成される()
    {
        // Arrange
        var players = PlayersTestHelper.CreateTestPlayers(4);

        // Act
        var list = new PlayerList(players);

        // Assert
        Assert.Equal(4, list.Count);
    }

    [Fact]
    public void プレイヤー3人_ArgumentExceptionが発生する()
    {
        // Arrange
        var players = PlayersTestHelper.CreateTestPlayers(3);

        // Act
        var exception = Record.Exception(() => new PlayerList(players));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void プレイヤー5人_ArgumentExceptionが発生する()
    {
        // Arrange
        var players = PlayersTestHelper.CreateTestPlayers(5);

        // Act
        var exception = Record.Exception(() => new PlayerList(players));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void PlayerIndexでアクセス_対応するPlayerが取得できる()
    {
        // Arrange
        var players = PlayersTestHelper.CreateTestPlayers(4);
        var list = new PlayerList(players);

        // Act
        var p0 = list[new PlayerIndex(0)];
        var p3 = list[new PlayerIndex(3)];

        // Assert
        Assert.Equal(players[0], p0);
        Assert.Equal(players[3], p3);
    }
}
