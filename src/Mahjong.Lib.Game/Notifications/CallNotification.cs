using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 副露通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="MadeCall">行われた副露</param>
/// <param name="CallerIndex">副露したプレイヤー</param>
public record CallNotification(
    PlayerRoundView View,
    Call MadeCall,
    PlayerIndex CallerIndex
) : RoundNotification(View);
