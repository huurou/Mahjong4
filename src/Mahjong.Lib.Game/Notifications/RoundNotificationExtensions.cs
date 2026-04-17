using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局内通知 (RoundNotification) を Wire DTO (PlayerNotification) へ変換する拡張メソッド群
/// </summary>
public static class RoundNotificationExtensions
{
    /// <summary>
    /// 局内通知を Wire DTO に変換する
    /// RoundNotification は View を持ち 行動選択系通知は CandidateList も持つ
    /// </summary>
    public static PlayerNotification ToWire(
        this RoundNotification notification,
        Guid notificationId,
        int roundRevision,
        PlayerIndex recipientIndex,
        TimeSpan timeout
    )
    {
        ArgumentNullException.ThrowIfNull(notification);

        var (type, candidates) = notification switch
        {
            HaipaiNotification => (NotificationType.Haipai, new CandidateList()),
            TsumoNotification n => (NotificationType.Tsumo, n.CandidateList),
            OtherPlayerTsumoNotification => (NotificationType.OtherPlayerTsumo, new CandidateList()),
            DahaiNotification n => (NotificationType.Dahai, n.CandidateList),
            CallNotification => (NotificationType.Call, new CandidateList()),
            KanNotification n => (NotificationType.Kan, n.CandidateList),
            KanTsumoNotification n => (NotificationType.KanTsumo, n.CandidateList),
            DoraRevealNotification => (NotificationType.DoraReveal, new CandidateList()),
            WinNotification => (NotificationType.Win, new CandidateList()),
            RyuukyokuNotification => (NotificationType.Ryuukyoku, new CandidateList()),
            _ => throw new ArgumentException($"未対応の RoundNotification です。実際:{notification.GetType().Name}", nameof(notification)),
        };
        return new PlayerNotification(
            notificationId,
            type,
            roundRevision,
            recipientIndex,
            notification.View,
            candidates,
            timeout
        );
    }
}
