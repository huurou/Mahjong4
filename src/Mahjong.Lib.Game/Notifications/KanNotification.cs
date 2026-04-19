using System.Collections.Immutable;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 槓通知 (加槓時に槍槓の可否を問う)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="KanCall">行われた槓</param>
/// <param name="KanCallerIndex">槓を宣言したプレイヤー</param>
/// <param name="CandidateList">合法応答候補 (ロン(槍槓)/OK)</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (非手番 3 人)</param>
public record KanNotification(
    PlayerRoundView View,
    Call KanCall,
    PlayerIndex KanCallerIndex,
    CandidateList CandidateList,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
