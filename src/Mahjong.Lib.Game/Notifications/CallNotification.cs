using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 副露通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="MadeCall">行われた副露</param>
/// <param name="CallerIndex">副露したプレイヤー</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (観測通知のため常に空)</param>
public record CallNotification(
    PlayerRoundView View,
    Call MadeCall,
    PlayerIndex CallerIndex,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
