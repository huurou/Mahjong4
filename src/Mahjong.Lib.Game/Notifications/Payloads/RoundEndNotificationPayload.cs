using Mahjong.Lib.Game.Decisions;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 局終了通知のペイロード
/// </summary>
/// <param name="Result">局終了結果 (和了 or 流局)</param>
public record RoundEndNotificationPayload(ResolvedRoundAction Result) : NotificationPayload;
