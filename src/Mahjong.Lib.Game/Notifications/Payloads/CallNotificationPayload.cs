using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 副露通知のペイロード
/// </summary>
/// <param name="MadeCall">行われた副露</param>
/// <param name="CallerIndex">副露したプレイヤー</param>
public record CallNotificationPayload(
    Call MadeCall,
    PlayerIndex CallerIndex
) : NotificationPayload;
