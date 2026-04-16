using Mahjong.Lib.Game.Decisions;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局終了通知
/// </summary>
/// <param name="Result">局終了結果 (和了 or 流局)</param>
public record RoundEndNotification(ResolvedRoundAction Result) : GameNotification;
