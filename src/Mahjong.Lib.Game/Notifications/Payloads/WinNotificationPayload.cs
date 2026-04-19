using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 和了通知のペイロード
/// </summary>
/// <param name="WinResult">和了結果</param>
public record WinNotificationPayload(AdoptedWinAction WinResult) : NotificationPayload;
