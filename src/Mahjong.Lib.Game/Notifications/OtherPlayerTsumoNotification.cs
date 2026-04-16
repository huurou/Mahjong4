using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 他家ツモ通知 (非手番プレイヤー用)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="TsumoPlayerIndex">ツモしたプレイヤー</param>
public record OtherPlayerTsumoNotification(
    PlayerRoundView View,
    PlayerIndex TsumoPlayerIndex
) : RoundNotification(View);
