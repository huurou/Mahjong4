using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 対局終了通知
/// </summary>
/// <param name="FinalPointArray">最終持ち点</param>
public record GameEndNotification(PointArray FinalPointArray) : GameNotification;
