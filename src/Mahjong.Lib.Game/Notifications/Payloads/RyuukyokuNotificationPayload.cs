using Mahjong.Lib.Game.Adoptions;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 流局通知のペイロード
/// </summary>
/// <param name="RyuukyokuResult">流局結果</param>
public record RyuukyokuNotificationPayload(
    AdoptedRyuukyokuAction RyuukyokuResult
) : NotificationPayload;
