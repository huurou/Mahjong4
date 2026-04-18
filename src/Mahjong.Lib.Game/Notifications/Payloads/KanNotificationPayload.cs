using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 槓通知のペイロード
/// </summary>
/// <param name="KanCall">行われた槓</param>
/// <param name="KanCallerIndex">槓を宣言したプレイヤー</param>
public record KanNotificationPayload(
    Call KanCall,
    PlayerIndex KanCallerIndex
) : NotificationPayload;
