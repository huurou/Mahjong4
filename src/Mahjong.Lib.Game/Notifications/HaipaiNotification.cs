using System.Collections.Immutable;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 配牌通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (観測通知のため常に空)</param>
public record HaipaiNotification(
    PlayerRoundView View,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
