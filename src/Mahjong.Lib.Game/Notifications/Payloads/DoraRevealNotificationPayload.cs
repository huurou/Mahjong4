using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// ドラ表示通知のペイロード
/// </summary>
/// <param name="NewDoraIndicator">新たに表示されたドラ表示牌</param>
public record DoraRevealNotificationPayload(Tile NewDoraIndicator) : NotificationPayload;
