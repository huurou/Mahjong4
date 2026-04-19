using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications.Payloads;
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
    /// 通知種別固有ペイロードは NotificationPayload 派生型として Payload に格納される
    /// </summary>
    public static PlayerNotification ToWire(
        this GameNotification notification,
        NotificationId notificationId,
        int roundRevision,
        PlayerIndex recipientIndex,
        TimeSpan timeout
    )
    {
        NotificationPayload payload = notification switch
        {
            GameStartNotification n => new GameStartNotificationPayload(n.PlayerList, n.Rules),
            RoundStartNotification n => new RoundStartNotificationPayload(n.RoundWind, n.RoundNumber, n.Honba, n.DealerIndex),
            RoundEndNotification n => new RoundEndNotificationPayload(n.Result),
            GameEndNotification n => new GameEndNotificationPayload(n.FinalPointArray),
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
            roundRevision,
            recipientIndex,
            null,
            [new OkCandidate()],
            timeout,
            [],
            payload
        );
    }
}
