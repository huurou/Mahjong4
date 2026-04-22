using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 副露後打牌要求通知のペイロード
/// </summary>
/// <param name="CalledTile">副露で取得した牌</param>
public record AfterCallNotificationPayload(Tile CalledTile) : NotificationPayload;
