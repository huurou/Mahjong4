using Mahjong.Lib.Game.Decisions;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 和了通知のペイロード
/// </summary>
/// <param name="WinResult">和了結果</param>
public record WinNotificationPayload(ResolvedWinAction WinResult) : NotificationPayload;
