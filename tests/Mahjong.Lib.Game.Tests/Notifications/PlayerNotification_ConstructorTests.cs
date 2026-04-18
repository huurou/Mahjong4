using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Payloads;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class PlayerNotification_ConstructorTests
{
    [Fact]
    public void WireDTO_全フィールドが保持される()
    {
        // Arrange
        var id = NotificationId.NewId();
        CandidateList candidates = [new OkCandidate()];
        var payload = new HaipaiNotificationPayload();

        // Act
        var notification = new PlayerNotification(
            id,
            1,
            new PlayerIndex(0),
            null,
            candidates,
            TimeSpan.FromSeconds(30),
            payload
        );

        // Assert
        Assert.Equal(id, notification.NotificationId);
        Assert.Equal(1, notification.RoundRevision);
        Assert.Equal(new PlayerIndex(0), notification.RecipientIndex);
        Assert.Null(notification.View);
        Assert.Single(notification.CandidateList);
        Assert.Equal(TimeSpan.FromSeconds(30), notification.Timeout);
        Assert.Same(payload, notification.Payload);
    }

    [Fact]
    public void Payloadがnull_ArgumentNullExceptionが発生する()
    {
        // Arrange
        var id = NotificationId.NewId();
        CandidateList candidates = [new OkCandidate()];

        // Act
        var ex = Record.Exception(() => new PlayerNotification(
            id,
            1,
            new PlayerIndex(0),
            null,
            candidates,
            TimeSpan.FromSeconds(30),
            null!
        ));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }
}
