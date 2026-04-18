using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// トレースを何も行わない no-op 実装
/// </summary>
public sealed class NullGameTracer : IGameTracer
{
    public static NullGameTracer Instance { get; } = new();

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification) { }
    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification) { }
    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response) { }
    public void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex) { }
    public void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex) { }
    public void OnResolvedAction(RoundDecisionPhase phase, ResolvedPlayerResponse resolved) { }
    public void OnRoundStarted(Round round) { }
    public void OnRoundEnded(ResolvedRoundAction action) { }
}
