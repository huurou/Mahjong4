using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 配牌通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
public record HaipaiNotification(PlayerRoundView View) : RoundNotification(View);
