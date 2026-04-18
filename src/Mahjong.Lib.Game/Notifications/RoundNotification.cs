using System.Text.Json.Serialization;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局内通知の共通基底 (C# API 層)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
[JsonDerivedType(typeof(HaipaiNotification), nameof(HaipaiNotification))]
[JsonDerivedType(typeof(TsumoNotification), nameof(TsumoNotification))]
[JsonDerivedType(typeof(OtherPlayerTsumoNotification), nameof(OtherPlayerTsumoNotification))]
[JsonDerivedType(typeof(DahaiNotification), nameof(DahaiNotification))]
[JsonDerivedType(typeof(CallNotification), nameof(CallNotification))]
[JsonDerivedType(typeof(KanNotification), nameof(KanNotification))]
[JsonDerivedType(typeof(KanTsumoNotification), nameof(KanTsumoNotification))]
[JsonDerivedType(typeof(WinNotification), nameof(WinNotification))]
[JsonDerivedType(typeof(RyuukyokuNotification), nameof(RyuukyokuNotification))]
[JsonDerivedType(typeof(DoraRevealNotification), nameof(DoraRevealNotification))]
public abstract record RoundNotification(PlayerRoundView View);
