using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 副露後打牌要求通知 (副露者 = 現手番プレイヤー用)。
/// チー/ポン成立直後に副露者に打牌選択を求める。
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="CalledTile">副露で取得した牌 (他家が捨てた牌)</param>
/// <param name="CandidateList">合法応答候補 (打牌のみ)</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (手番 1 人)</param>
public record AfterCallNotification(
    PlayerRoundView View,
    Tile CalledTile,
    CandidateList CandidateList,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
