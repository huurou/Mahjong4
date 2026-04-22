using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 嶺上ツモ通知 (手番プレイヤー用)
/// RoundStateKanTsumo + RoundStateAfterKanTsumo を1通知にまとめる
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="DrawnTile">嶺上ツモ牌</param>
/// <param name="CandidateList">合法応答候補 (嶺上ツモ和了/打牌/暗槓/加槓)</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (手番 1 人)</param>
public record KanTsumoNotification(
    PlayerRoundView View,
    Tile DrawnTile,
    CandidateList CandidateList,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
