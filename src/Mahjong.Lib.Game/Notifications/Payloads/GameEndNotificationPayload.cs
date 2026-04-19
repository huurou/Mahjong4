using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 対局終了通知のペイロード
/// </summary>
/// <param name="FinalPointArray">最終持ち点</param>
public record GameEndNotificationPayload(PointArray FinalPointArray) : NotificationPayload;
