using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tests.Players;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameManager_GameLevelNotificationTests
{
    private static (GameManager Manager, FakePlayer[] Players) CreateManagerAndPlayers()
    {
        var players = new[]
        {
            FakePlayer.Create(0),
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        var playerList = new PlayerList(players);
        var manager = GamesTestHelper.CreateManager(playerList);
        return (manager, players);
    }

    [Fact]
    public async Task StartAsync_GameStartNotificationが全員に送信される()
    {
        // Arrange
        var (manager, players) = CreateManagerAndPlayers();
        using var _ = manager;

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        foreach (var player in players)
        {
            Assert.Contains(player.ReceivedNotifications, x => x is GameStartNotification);
        }
    }

    [Fact]
    public async Task StartAsync_RoundStartNotificationが全員に送信される()
    {
        // Arrange
        var (manager, players) = CreateManagerAndPlayers();
        using var _ = manager;

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        foreach (var player in players)
        {
            Assert.Contains(player.ReceivedNotifications, x => x is RoundStartNotification);
        }
    }

    [Fact]
    public async Task StartAsync_GameStartNotificationの受信者インデックスは各プレイヤーと一致()
    {
        // Arrange
        var (manager, players) = CreateManagerAndPlayers();
        using var _ = manager;

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        for (var i = 0; i < 4; i++)
        {
            var notification = players[i].ReceivedNotifications
                .OfType<GameStartNotification>()
                .First();
            Assert.Equal(new PlayerIndex(i), notification.RecipientIndex);
        }
    }
}
