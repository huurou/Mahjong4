using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 流局通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="RyuukyokuResult">流局結果</param>
public record RyuukyokuNotification(
    PlayerRoundView View,
    AdoptedRyuukyokuAction RyuukyokuResult
) : RoundNotification(View);
