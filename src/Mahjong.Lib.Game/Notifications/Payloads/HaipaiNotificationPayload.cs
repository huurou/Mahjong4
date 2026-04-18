namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 配牌通知のペイロード (シンプルな ACK 通知のため固有情報を持たない)
/// </summary>
public record HaipaiNotificationPayload : NotificationPayload;
