using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 他家ツモ通知のペイロード
/// </summary>
/// <param name="TsumoPlayerIndex">ツモしたプレイヤー</param>
public record OtherPlayerTsumoNotificationPayload(
    PlayerIndex TsumoPlayerIndex
) : NotificationPayload;
