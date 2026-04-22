using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// トレースを何も行わない no-op 実装
/// </summary>
public sealed class NullGameTracer : IGameTracer
{
    public static NullGameTracer Instance { get; } = new();

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification)
    {
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
    }

    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response)
    {
    }

    public void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex)
    {
    }

    public void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex)
    {
    }

    public void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates)
    {
    }

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
    }

    public void OnRoundStarted(Round round)
    {
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
    }

    public void OnTsumoDrawn(PlayerIndex turn, Tile drawnTile, bool isRinshan)
    {
    }

    public void OnDoraRevealed(Tile newIndicator)
    {
    }

    public void OnRiichiDeclared(PlayerIndex player, int step)
    {
    }

    public void OnCallExecuted(PlayerIndex caller, Call call)
    {
    }
}
