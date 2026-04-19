using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 他家嶺上ツモ通知のペイロード
/// </summary>
/// <param name="KanTsumoPlayerIndex">嶺上ツモしたプレイヤー</param>
public record OtherPlayerKanTsumoNotificationPayload(
    PlayerIndex KanTsumoPlayerIndex
) : NotificationPayload;
