using System.Collections.Immutable;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 和了通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="WinResult">和了結果</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (観測通知のため常に空)</param>
public record WinNotification(
    PlayerRoundView View,
    AdoptedWinAction WinResult,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
