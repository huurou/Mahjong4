using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局内通知の共通基底 (C# API 層)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (= 非 OK 応答を返せるプレイヤー)。
/// 空なら観測通知で全員 OK 応答のみ。観測通知でも全プレイヤーが OK 応答を返し、サーバーはそれらを待つ</param>
[JsonDerivedType(typeof(HaipaiNotification), nameof(HaipaiNotification))]
[JsonDerivedType(typeof(TsumoNotification), nameof(TsumoNotification))]
[JsonDerivedType(typeof(OtherPlayerTsumoNotification), nameof(OtherPlayerTsumoNotification))]
[JsonDerivedType(typeof(DahaiNotification), nameof(DahaiNotification))]
[JsonDerivedType(typeof(CallNotification), nameof(CallNotification))]
[JsonDerivedType(typeof(AfterCallNotification), nameof(AfterCallNotification))]
[JsonDerivedType(typeof(OtherPlayerAfterCallNotification), nameof(OtherPlayerAfterCallNotification))]
[JsonDerivedType(typeof(KanNotification), nameof(KanNotification))]
[JsonDerivedType(typeof(KanTsumoNotification), nameof(KanTsumoNotification))]
[JsonDerivedType(typeof(OtherPlayerKanTsumoNotification), nameof(OtherPlayerKanTsumoNotification))]
[JsonDerivedType(typeof(WinNotification), nameof(WinNotification))]
[JsonDerivedType(typeof(RyuukyokuNotification), nameof(RyuukyokuNotification))]
[JsonDerivedType(typeof(DoraRevealNotification), nameof(DoraRevealNotification))]
public abstract record RoundNotification(
    PlayerRoundView View,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
);
