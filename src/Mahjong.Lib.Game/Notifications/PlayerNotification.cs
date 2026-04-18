using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications.Payloads;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// プレイヤー通知 Wire DTO (シリアライズ/通信用の共通エンベロープ)
/// </summary>
public record PlayerNotification
{
    /// <summary>
    /// 通知ユニークID (PN_uuidv7 形式)
    /// </summary>
    public NotificationId NotificationId { get; init; }

    /// <summary>
    /// 局内連番 (古い応答の検出・リプレイ用)
    /// </summary>
    public int RoundRevision { get; init; }

    /// <summary>
    /// 通知先プレイヤー
    /// </summary>
    public PlayerIndex RecipientIndex { get; init; }

    /// <summary>
    /// プレイヤー視点フィルタ済み卓情報 (対局レベル通知では null)
    /// </summary>
    public PlayerRoundView? View { get; init; }

    /// <summary>
    /// 合法応答候補。常に 1 件以上を含む (OK 応答のみ許容する通知でも <see cref="OkCandidate"/> を 1 件含める)。空リストは不正
    /// </summary>
    public CandidateList CandidateList
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Count == 0)
            {
                throw new ArgumentException(
                    "CandidateList は 1 件以上の合法応答候補を含む必要があります。OK 応答のみ許容する通知でも OkCandidate を含めてください。",
                    nameof(value)
                );
            }
            field = value;
        }
    }

    /// <summary>
    /// 応答タイムアウト
    /// </summary>
    public TimeSpan Timeout { get; init; }

    /// <summary>
    /// 通知種別固有ペイロード。判別は Payload の具象型で行う
    /// </summary>
    public NotificationPayload Payload { get; init; }

    public PlayerNotification(
        NotificationId notificationId,
        int roundRevision,
        PlayerIndex recipientIndex,
        PlayerRoundView? view,
        CandidateList candidateList,
        TimeSpan timeout,
        NotificationPayload payload
    )
    {
        ArgumentNullException.ThrowIfNull(payload);

        NotificationId = notificationId;
        RoundRevision = roundRevision;
        RecipientIndex = recipientIndex;
        View = view;
        CandidateList = candidateList;
        Timeout = timeout;
        Payload = payload;
    }
}
