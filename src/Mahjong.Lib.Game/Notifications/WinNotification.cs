using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 和了通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="WinResult">和了結果</param>
public record WinNotification(
    PlayerRoundView View,
    ResolvedWinAction WinResult
) : RoundNotification(View);
