using Mahjong.Lib.Game.Decisions;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 流局通知のペイロード
/// </summary>
/// <param name="RyuukyokuResult">流局結果</param>
public record RyuukyokuNotificationPayload(
    ResolvedRyuukyokuAction RyuukyokuResult
) : NotificationPayload;
