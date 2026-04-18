using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications.Payloads;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局内通知 (RoundNotification) を Wire DTO (PlayerNotification) へ変換する拡張メソッド群
/// </summary>
public static class RoundNotificationExtensions
{
    /// <summary>
    /// 局内通知を Wire DTO に変換する
    /// 通知種別固有ペイロードは NotificationPayload 派生型として Payload に格納される
    /// OK 応答のみ許容する通知は CandidateList に OkCandidate を 1 件含める
    /// </summary>
    public static PlayerNotification ToWire(
        this RoundNotification notification,
        NotificationId notificationId,
        int roundRevision,
        PlayerIndex recipientIndex,
        TimeSpan timeout
    )
    {
        ArgumentNullException.ThrowIfNull(notification);

        CandidateList okOnly = [new OkCandidate()];
        var (candidates, payload) = notification switch
        {
            HaipaiNotification => (okOnly, (NotificationPayload)new HaipaiNotificationPayload()),
            TsumoNotification n => (n.CandidateList, new TsumoNotificationPayload(n.TsumoTile)),
            OtherPlayerTsumoNotification n => (okOnly, new OtherPlayerTsumoNotificationPayload(n.TsumoPlayerIndex)),
            DahaiNotification n => (n.CandidateList, new DahaiNotificationPayload(n.DiscardedTile, n.DiscarderIndex)),
            CallNotification n => (okOnly, new CallNotificationPayload(n.MadeCall, n.CallerIndex)),
            KanNotification n => (n.CandidateList, new KanNotificationPayload(n.KanCall, n.KanCallerIndex)),
            KanTsumoNotification n => (n.CandidateList, new KanTsumoNotificationPayload(n.DrawnTile)),
            DoraRevealNotification n => (okOnly, new DoraRevealNotificationPayload(n.NewDoraIndicator)),
            WinNotification n => (okOnly, new WinNotificationPayload(n.WinResult)),
            RyuukyokuNotification n => (okOnly, new RyuukyokuNotificationPayload(n.RyuukyokuResult)),
            _ => throw new ArgumentException($"未対応の RoundNotification です。実際:{notification.GetType().Name}", nameof(notification)),
        };
        return new PlayerNotification(
            notificationId,
            roundRevision,
            recipientIndex,
            notification.View,
            candidates,
            timeout,
            payload
        );
    }
}
