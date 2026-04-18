using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// ツモ通知のペイロード
/// </summary>
/// <param name="TsumoTile">ツモ牌</param>
public record TsumoNotificationPayload(Tile TsumoTile) : NotificationPayload;
