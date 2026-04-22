using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 他家副露後打牌要求通知のペイロード
/// </summary>
/// <param name="CallerIndex">副露した (= 打牌選択中の) プレイヤー</param>
public record OtherPlayerAfterCallNotificationPayload(
    PlayerIndex CallerIndex
) : NotificationPayload;
