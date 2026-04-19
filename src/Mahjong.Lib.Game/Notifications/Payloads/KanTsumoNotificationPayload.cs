using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 嶺上ツモ通知のペイロード
/// </summary>
/// <param name="DrawnTile">嶺上ツモ牌</param>
public record KanTsumoNotificationPayload(Tile DrawnTile) : NotificationPayload;
