using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 打牌通知のペイロード
/// </summary>
/// <param name="DiscardedTile">打牌された牌</param>
/// <param name="DiscarderIndex">打牌者</param>
public record DahaiNotificationPayload(
    Tile DiscardedTile,
    PlayerIndex DiscarderIndex
) : NotificationPayload;
