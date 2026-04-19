using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
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
    /// <summary>
    /// プレイヤー応答が提示済み候補に含まれない候補外応答であった場合に呼ばれる。
    /// RoundStateContext の通知・応答集約ループは <see cref="IDefaultResponseFactory"/> のフォールバック応答に差し替えて進行を継続する
    /// </summary>
    void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates);
    void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted);
    void OnRoundStarted(Round round);
    void OnRoundEnded(AdoptedRoundAction action);
}
