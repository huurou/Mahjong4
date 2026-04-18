using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 対局進行のイベントトレース
/// 観測用の副作用のみを持ち、通知・応答フローに影響を与えない
/// </summary>
public interface IGameTracer
{
    void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification);
    void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification);
    void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response);
    void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex);
    void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex);
    void OnResolvedAction(RoundDecisionPhase phase, ResolvedPlayerResponse resolved);
    void OnRoundStarted(Round round);
    void OnRoundEnded(ResolvedRoundAction action);
}
