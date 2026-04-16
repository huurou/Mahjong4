using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// プレイヤー通知 Wire DTO (シリアライズ/通信用の共通エンベロープ)
/// </summary>
/// <param name="NotificationId">通知ユニークID (UUIDv7)</param>
/// <param name="Type">通知種別</param>
/// <param name="RoundRevision">局内連番 (古い応答の検出・リプレイ用)</param>
/// <param name="RecipientIndex">通知先プレイヤー</param>
/// <param name="View">プレイヤー視点フィルタ済み卓情報 (局外通知では null)</param>
/// <param name="CandidateList">合法応答候補 (OKのみの場面もある)</param>
/// <param name="Timeout">応答タイムアウト</param>
public record PlayerNotification(
    Guid NotificationId,
    NotificationType Type,
    int RoundRevision,
    PlayerIndex RecipientIndex,
    PlayerRoundView? View,
    CandidateList CandidateList,
    TimeSpan Timeout
);
