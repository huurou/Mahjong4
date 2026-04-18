using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 対局レベル通知の共通基底
/// </summary>
[JsonDerivedType(typeof(GameStartNotification), nameof(GameStartNotification))]
[JsonDerivedType(typeof(GameEndNotification), nameof(GameEndNotification))]
[JsonDerivedType(typeof(RoundStartNotification), nameof(RoundStartNotification))]
[JsonDerivedType(typeof(RoundEndNotification), nameof(RoundEndNotification))]
public abstract record GameNotification;
