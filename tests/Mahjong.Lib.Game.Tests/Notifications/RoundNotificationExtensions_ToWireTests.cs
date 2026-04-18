using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Payloads;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class RoundNotificationExtensions_ToWireTests
{
    private const int ROUND_REVISION = 3;

    private static NotificationId NotificationId { get; } = NotificationId.NewId();
    private static PlayerIndex RecipientIndex { get; } = new(0);
    private static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

    [Fact]
    public void HaipaiNotification_PayloadがHaipaiNotificationPayload_Viewが保持される()
    {
        // Arrange
        var view = CreateView();
        var notification = new HaipaiNotification(view);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        Assert.IsType<HaipaiNotificationPayload>(wire.Payload);
        Assert.Equal(view, wire.View);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
    }

    [Fact]
    public void TsumoNotification_PayloadがTsumoNotificationPayloadでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var tsumoTile = new Tile(0);
        var candidates = new CandidateList([new TsumoAgariCandidate()]);
        var notification = new TsumoNotification(view, tsumoTile, candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<TsumoNotificationPayload>(wire.Payload);
        Assert.Equal(tsumoTile, payload.TsumoTile);
        Assert.Equal(view, wire.View);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void OtherPlayerTsumoNotification_PayloadがOtherPlayerTsumoNotificationPayload()
    {
        // Arrange
        var view = CreateView();
        var tsumoPlayerIndex = new PlayerIndex(1);
        var notification = new OtherPlayerTsumoNotification(view, tsumoPlayerIndex);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<OtherPlayerTsumoNotificationPayload>(wire.Payload);
        Assert.Equal(tsumoPlayerIndex, payload.TsumoPlayerIndex);
        Assert.Equal(view, wire.View);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
    }

    [Fact]
    public void DahaiNotification_PayloadがDahaiNotificationPayloadでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var discardedTile = new Tile(4);
        var discarderIndex = new PlayerIndex(1);
        var candidates = new CandidateList([new OkCandidate(), new RonCandidate()]);
        var notification = new DahaiNotification(view, discardedTile, discarderIndex, candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<DahaiNotificationPayload>(wire.Payload);
        Assert.Equal(discardedTile, payload.DiscardedTile);
        Assert.Equal(discarderIndex, payload.DiscarderIndex);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void CallNotification_PayloadがCallNotificationPayload()
    {
        // Arrange
        var view = CreateView();
        var call = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(2)], new PlayerIndex(0), new Tile(2));
        var callerIndex = new PlayerIndex(0);
        var notification = new CallNotification(view, call, callerIndex);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<CallNotificationPayload>(wire.Payload);
        Assert.Equal(call, payload.MadeCall);
        Assert.Equal(callerIndex, payload.CallerIndex);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
    }

    [Fact]
    public void KanNotification_PayloadがKanNotificationPayloadでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var call = new Call(CallType.Kakan, [new Tile(0), new Tile(1), new Tile(2), new Tile(3)], new PlayerIndex(0), new Tile(3));
        var callerIndex = new PlayerIndex(0);
        var candidates = new CandidateList([new OkCandidate()]);
        var notification = new KanNotification(view, call, callerIndex, candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<KanNotificationPayload>(wire.Payload);
        Assert.Equal(call, payload.KanCall);
        Assert.Equal(callerIndex, payload.KanCallerIndex);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void KanTsumoNotification_PayloadがKanTsumoNotificationPayloadでCandidateListが保持される()
    {
        // Arrange
        var view = CreateView();
        var drawnTile = new Tile(0);
        var candidates = new CandidateList([new TsumoAgariCandidate()]);
        var notification = new KanTsumoNotification(view, drawnTile, candidates);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<KanTsumoNotificationPayload>(wire.Payload);
        Assert.Equal(drawnTile, payload.DrawnTile);
        Assert.Equal(candidates, wire.CandidateList);
    }

    [Fact]
    public void DoraRevealNotification_PayloadがDoraRevealNotificationPayload()
    {
        // Arrange
        var view = CreateView();
        var indicator = new Tile(0);
        var notification = new DoraRevealNotification(view, indicator);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<DoraRevealNotificationPayload>(wire.Payload);
        Assert.Equal(indicator, payload.NewDoraIndicator);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
    }

    [Fact]
    public void WinNotification_PayloadがWinNotificationPayload()
    {
        // Arrange
        var view = CreateView();
        var winResult = CreateResolvedWinAction();
        var notification = new WinNotification(view, winResult);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<WinNotificationPayload>(wire.Payload);
        Assert.Equal(winResult, payload.WinResult);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
    }

    [Fact]
    public void RyuukyokuNotification_PayloadがRyuukyokuNotificationPayload()
    {
        // Arrange
        var view = CreateView();
        var ryuukyokuResult = CreateResolvedRyuukyokuAction();
        var notification = new RyuukyokuNotification(view, ryuukyokuResult);

        // Act
        var wire = notification.ToWire(NotificationId, ROUND_REVISION, RecipientIndex, Timeout);

        // Assert
        var payload = Assert.IsType<RyuukyokuNotificationPayload>(wire.Payload);
        Assert.Equal(ryuukyokuResult, payload.RyuukyokuResult);
        Assert.Contains(new OkCandidate(), wire.CandidateList);
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
