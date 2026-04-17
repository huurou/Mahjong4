using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class RoundNotificationExtensions_ToWireTests
{
    private const int ROUND_REVISION = 3;

    private static Guid NotificationId { get; } = Guid.NewGuid();
    private static PlayerIndex RecipientIndex { get; } = new(0);
    private static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

    [Fact]
    public void HaipaiNotification_TypeがHaipaiでViewが保持される()
    {
        // Arrange
        var view = CreateView();
        var notification = new HaipaiNotification(view);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Haipai, wire.Type);
        Assert.Equal(view, wire.View);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void TsumoNotification_TypeがTsumoでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var candidates = new CandidateList([new TsumoAgariCandidate()]);
        var notification = new TsumoNotification(view, new Tile(0), candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Tsumo, wire.Type);
        Assert.Equal(view, wire.View);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void OtherPlayerTsumoNotification_TypeがOtherPlayerTsumo()
    {
        // Arrange
        var view = CreateView();
        var notification = new OtherPlayerTsumoNotification(view, new PlayerIndex(1));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.OtherPlayerTsumo, wire.Type);
        Assert.Equal(view, wire.View);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void DahaiNotification_TypeがDahaiでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var candidates = new CandidateList([new OkCandidate(), new RonCandidate()]);
        var notification = new DahaiNotification(view, new Tile(4), new PlayerIndex(1), candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Dahai, wire.Type);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void CallNotification_TypeがCall()
    {
        // Arrange
        var view = CreateView();
        var call = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(2)], new PlayerIndex(0), new Tile(2));
        var notification = new CallNotification(view, call, new PlayerIndex(0));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Call, wire.Type);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void KanNotification_TypeがKanでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var call = new Call(CallType.Kakan, [new Tile(0), new Tile(1), new Tile(2), new Tile(3)], new PlayerIndex(0), new Tile(3));
        var candidates = new CandidateList([new OkCandidate()]);
        var notification = new KanNotification(view, call, new PlayerIndex(0), candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Kan, wire.Type);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void KanTsumoNotification_TypeがKanTsumoでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var candidates = new CandidateList([new TsumoAgariCandidate()]);
        var notification = new KanTsumoNotification(view, new Tile(0), candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.KanTsumo, wire.Type);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void DoraRevealNotification_TypeがDoraReveal()
    {
        // Arrange
        var view = CreateView();
        var notification = new DoraRevealNotification(view, new Tile(0));

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.DoraReveal, wire.Type);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void WinNotification_TypeがWin()
    {
        // Arrange
        var view = CreateView();
        var notification = new WinNotification(view, CreateResolvedWinAction());

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Win, wire.Type);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void RyuukyokuNotification_TypeがRyuukyoku()
    {
        // Arrange
        var view = CreateView();
        var notification = new RyuukyokuNotification(view, CreateResolvedRyuukyokuAction());

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.Equal(NotificationType.Ryuukyoku, wire.Type);
        Assert.Empty(wire.CandidateList);
    }

    [Fact]
    public void Notificationがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        RoundNotification notification = null!;
        var ex = Record.Exception(() => notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }

    private static PlayerRoundView CreateView()
    {
        return new PlayerRoundView(
            new PlayerIndex(0),
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(35000)),
            new Hand(),
            new CallListArray(),
            new RiverArray(),
            [],
            new OwnRoundStatus(false, false, false, true, false, false, false),
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true),
            ],
            70
        );
    }

    private static ResolvedWinAction CreateResolvedWinAction()
    {
        var scoreResult = new ScoreResult(1, 30, new PointArray(new Point(35000)), []);
        var winner = new ResolvedWinner(new PlayerIndex(0), new Tile(0), scoreResult);
        return new ResolvedWinAction(
            [winner],
            null,
            WinType.Tsumo,
            null,
            new Honba(0),
            false
        );
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
