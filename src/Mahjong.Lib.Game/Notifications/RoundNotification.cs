using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局内通知の共通基底 (C# API 層)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
public abstract record RoundNotification(PlayerRoundView View);
