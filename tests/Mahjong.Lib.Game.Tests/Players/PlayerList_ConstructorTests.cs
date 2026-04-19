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
    public void 位置とPlayerIndexが不一致_ArgumentExceptionが発生する()
    {
        // Arrange: 4人揃うが PlayerIndex が位置と不一致 (index 0 に PlayerIndex(1) を持つプレイヤー)
        var players = new Player[]
        {
            new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "P0", new PlayerIndex(1)),
            new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "P1", new PlayerIndex(1)),
            new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "P2", new PlayerIndex(2)),
            new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "P3", new PlayerIndex(3)),
        };

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
