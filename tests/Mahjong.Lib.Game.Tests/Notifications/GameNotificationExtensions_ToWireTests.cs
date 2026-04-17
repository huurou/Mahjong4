using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class GameNotificationExtensions_ToWireTests
{
    private const int ROUND_REVISION = 3;

    private static Guid NotificationId { get; } = Guid.NewGuid();
    private static PlayerIndex RecipientIndex { get; } = new(0);
    private static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

    [Fact]
    public void GameStartNotification_TypeがGameStartでViewがnull()
    {
        // Arrange
        var playerList = new PlayerList(Players.PlayersTestHelper.CreateTestPlayers(4));
        var rules = new GameRules();
        var notification = new GameStartNotification(playerList, rules, RecipientIndex);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationId, wire.NotificationId);
        Assert.Equal(NotificationType.GameStart, wire.Type);
        Assert.Equal(ROUND_REVISION, wire.RoundRevision);
        Assert.Equal(RecipientIndex, wire.RecipientIndex);
        Assert.Null(wire.View);
        Assert.Empty(wire.CandidateList);
        Assert.Equal(Timeout, wire.Timeout);
    }

    [Fact]
    public void RoundStartNotification_TypeがRoundStart()
    {
        // Arrange
        var notification = new RoundStartNotification(RoundWind.East, new RoundNumber(0), new Honba(0), new PlayerIndex(0));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.RoundStart, wire.Type);
        Assert.Null(wire.View);
    }

    [Fact]
    public void RoundEndNotification_TypeがRoundEnd()
    {
        // Arrange
        var notification = new RoundEndNotification(CreateResolvedRyuukyokuAction());

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.RoundEnd, wire.Type);
        Assert.Null(wire.View);
    }

    [Fact]
    public void GameEndNotification_TypeがGameEnd()
    {
        // Arrange
        var notification = new GameEndNotification(new PointArray(new Point(35000)));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.GameEnd, wire.Type);
        Assert.Null(wire.View);
    }

    [Fact]
    public void Notificationがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        GameNotification notification = null!;
        var ex = Record.Exception(() => notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void GameStartNotificationのRecipientIndexと引数が不一致_ArgumentExceptionが発生する()
    {
        // Arrange
        var playerList = new PlayerList(Players.PlayersTestHelper.CreateTestPlayers(4));
        var rules = new GameRules();
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));

        // Act
        var ex = Record.Exception(() => notification.ToWire(NotificationId, ROUND_REVISION, new PlayerIndex(1), Timeout));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    private static ResolvedRyuukyokuAction CreateResolvedRyuukyokuAction()
    {
        return new ResolvedRyuukyokuAction(
            RyuukyokuType.KouhaiHeikyoku,
            [],
            [],
            false
        );
    }
}
