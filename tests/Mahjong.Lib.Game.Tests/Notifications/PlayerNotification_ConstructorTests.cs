using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class PlayerNotification_ConstructorTests
{
    [Fact]
    public void WireDTO_全フィールドが保持される()
    {
        // Arrange
        var id = Guid.NewGuid();
        CandidateList candidates = [new OkCandidate()];

        // Act
        var notification = new PlayerNotification(
            id,
            NotificationType.Haipai,
            1,
            new PlayerIndex(0),
            null,
            candidates,
            TimeSpan.FromSeconds(30)
        );

        // Assert
        Assert.Equal(id, notification.NotificationId);
        Assert.Equal(NotificationType.Haipai, notification.Type);
        Assert.Equal(1, notification.RoundRevision);
        Assert.Equal(new PlayerIndex(0), notification.RecipientIndex);
        Assert.Null(notification.View);
        Assert.Single(notification.CandidateList);
        Assert.Equal(TimeSpan.FromSeconds(30), notification.Timeout);
    }
}
