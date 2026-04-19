using System.Collections.Immutable;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// ドラ表示通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="NewDoraIndicator">新たに表示されたドラ表示牌</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (観測通知のため常に空)</param>
public record DoraRevealNotification(
    PlayerRoundView View,
    Tile NewDoraIndicator,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
