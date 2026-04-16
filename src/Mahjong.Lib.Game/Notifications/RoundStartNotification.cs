using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 局開始通知
/// </summary>
/// <param name="RoundWind">場風</param>
/// <param name="RoundNumber">局数</param>
/// <param name="Honba">本場</param>
/// <param name="DealerIndex">親のプレイヤーインデックス</param>
public record RoundStartNotification(
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    PlayerIndex DealerIndex
) : GameNotification;
