using System.Collections.Immutable;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// ツモ通知 (手番プレイヤー用)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="TsumoTile">ツモ牌</param>
/// <param name="CandidateList">合法応答候補 (打牌/暗槓/加槓/ツモ和了/九種九牌)</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (手番 1 人)</param>
public record TsumoNotification(
    PlayerRoundView View,
    Tile TsumoTile,
    CandidateList CandidateList,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
