using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 局開始通知のペイロード
/// </summary>
/// <param name="RoundWind">場風</param>
/// <param name="RoundNumber">局数</param>
/// <param name="Honba">本場</param>
/// <param name="DealerIndex">親のプレイヤーインデックス</param>
public record RoundStartNotificationPayload(
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    PlayerIndex DealerIndex
) : NotificationPayload;
