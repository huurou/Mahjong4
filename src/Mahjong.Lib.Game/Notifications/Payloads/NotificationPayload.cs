using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 通知固有ペイロードの基底型 (Wire DTO)
/// PlayerNotification.Payload に格納され、通知種別ごとの情報を運搬する
/// </summary>
[JsonDerivedType(typeof(HaipaiNotificationPayload), nameof(HaipaiNotificationPayload))]
[JsonDerivedType(typeof(TsumoNotificationPayload), nameof(TsumoNotificationPayload))]
[JsonDerivedType(typeof(OtherPlayerTsumoNotificationPayload), nameof(OtherPlayerTsumoNotificationPayload))]
[JsonDerivedType(typeof(DahaiNotificationPayload), nameof(DahaiNotificationPayload))]
[JsonDerivedType(typeof(CallNotificationPayload), nameof(CallNotificationPayload))]
[JsonDerivedType(typeof(KanNotificationPayload), nameof(KanNotificationPayload))]
[JsonDerivedType(typeof(KanTsumoNotificationPayload), nameof(KanTsumoNotificationPayload))]
[JsonDerivedType(typeof(OtherPlayerKanTsumoNotificationPayload), nameof(OtherPlayerKanTsumoNotificationPayload))]
[JsonDerivedType(typeof(WinNotificationPayload), nameof(WinNotificationPayload))]
[JsonDerivedType(typeof(RyuukyokuNotificationPayload), nameof(RyuukyokuNotificationPayload))]
[JsonDerivedType(typeof(DoraRevealNotificationPayload), nameof(DoraRevealNotificationPayload))]
[JsonDerivedType(typeof(GameStartNotificationPayload), nameof(GameStartNotificationPayload))]
[JsonDerivedType(typeof(RoundStartNotificationPayload), nameof(RoundStartNotificationPayload))]
[JsonDerivedType(typeof(RoundEndNotificationPayload), nameof(RoundEndNotificationPayload))]
[JsonDerivedType(typeof(GameEndNotificationPayload), nameof(GameEndNotificationPayload))]
public abstract record NotificationPayload;
