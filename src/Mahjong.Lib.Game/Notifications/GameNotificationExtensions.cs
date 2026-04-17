using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 対局レベル通知 (GameNotification) を Wire DTO (PlayerNotification) へ変換する拡張メソッド群
/// </summary>
public static class GameNotificationExtensions
{
    /// <summary>
    /// 対局レベル通知を Wire DTO に変換する
    /// GameNotification は視点卓情報 (View) を持たないため Wire DTO 側では null となる
    /// </summary>
    public static PlayerNotification ToWire(
        this GameNotification notification,
        Guid notificationId,
        int roundRevision,
        PlayerIndex recipientIndex,
        TimeSpan timeout
    )
    {
        ArgumentNullException.ThrowIfNull(notification);

        var type = notification switch
        {
            GameStartNotification => NotificationType.GameStart,
            RoundStartNotification => NotificationType.RoundStart,
            RoundEndNotification => NotificationType.RoundEnd,
            GameEndNotification => NotificationType.GameEnd,
            _ => throw new ArgumentException($"未対応の GameNotification です。実際:{notification.GetType().Name}", nameof(notification)),
        };

        if (notification is GameStartNotification gameStart && gameStart.RecipientIndex != recipientIndex)
        {
            throw new ArgumentException(
                $"GameStartNotification.RecipientIndex ({gameStart.RecipientIndex}) と ToWire 引数 recipientIndex ({recipientIndex}) が一致しません。",
                nameof(recipientIndex)
            );
        }
        return new PlayerNotification(
            notificationId,
            type,
            roundRevision,
            recipientIndex,
            View: null,
            CandidateList: [],
            timeout
        );
    }
}
