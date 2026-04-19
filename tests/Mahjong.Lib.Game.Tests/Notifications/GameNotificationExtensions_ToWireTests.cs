using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Payloads;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class GameNotificationExtensions_ToWireTests
{
    private const int ROUND_REVISION = 3;

    private static NotificationId NotificationId { get; } = NotificationId.NewId();
    private static PlayerIndex RecipientIndex { get; } = new(0);
    private static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

    [Fact]
    public void GameStartNotification_PayloadがGameStartNotificationPayloadでViewがnull()
    {
        // Arrange
        var playerList = new PlayerList(Players.PlayersTestHelper.CreateTestPlayers(4));
        var rules = new GameRules();
        var notification = new GameStartNotification(playerList, rules, RecipientIndex);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationId, wire.NotificationId);
        Assert.Equal(ROUND_REVISION, wire.RoundRevision);
        Assert.Equal(RecipientIndex, wire.RecipientIndex);
        Assert.Null(wire.View);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
        Assert.Equal(Timeout, wire.Timeout);
        var payload = Assert.IsType<GameStartNotificationPayload>(wire.Payload);
        Assert.Equal(playerList, payload.PlayerList);
        Assert.Equal(rules, payload.Rules);
    }

    [Fact]
    public void RoundStartNotification_PayloadがRoundStartNotificationPayload()
    {
        // Arrange
        var notification = new RoundStartNotification(RoundWind.East, new RoundNumber(0), new Honba(0), new PlayerIndex(0));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Null(wire.View);
        var payload = Assert.IsType<RoundStartNotificationPayload>(wire.Payload);
        Assert.Equal(RoundWind.East, payload.RoundWind);
        Assert.Equal(new RoundNumber(0), payload.RoundNumber);
        Assert.Equal(new Honba(0), payload.Honba);
        Assert.Equal(new PlayerIndex(0), payload.DealerIndex);
    }

    [Fact]
    public void RoundEndNotification_PayloadがRoundEndNotificationPayload()
    {
        // Arrange
        var result = CreateAdoptedRyuukyokuAction();
        var notification = new RoundEndNotification(result);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Null(wire.View);
        var payload = Assert.IsType<RoundEndNotificationPayload>(wire.Payload);
        Assert.Equal(result, payload.Result);
    }

    [Fact]
    public void GameEndNotification_PayloadがGameEndNotificationPayload()
    {
        // Arrange
        var finalPoints = new PointArray(new Point(35000));
        var notification = new GameEndNotification(finalPoints);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Null(wire.View);
        var payload = Assert.IsType<GameEndNotificationPayload>(wire.Payload);
        Assert.Equal(finalPoints, payload.FinalPointArray);
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

    private static AdoptedRyuukyokuAction CreateAdoptedRyuukyokuAction()
    {
        return new AdoptedRyuukyokuAction(
            RyuukyokuType.KouhaiHeikyoku,
            [],
            [],
            false
        );
    }
}
