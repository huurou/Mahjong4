using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Microsoft.Extensions.Logging;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 複数の <see cref="IGameTracer"/> を fan-out する集約トレーサー。
/// 個別トレーサーの例外は対局ループを壊さないよう握り潰し、logger に warn 出力する
/// </summary>
public sealed class CompositeGameTracer(
    IReadOnlyList<IGameTracer> tracers,
    ILogger<CompositeGameTracer>? logger = null
) : IGameTracer
{
    private readonly IReadOnlyList<IGameTracer> tracers_ = tracers;
    private readonly ILogger<CompositeGameTracer>? logger_ = logger;

    public CompositeGameTracer(params IGameTracer[] tracers) : this((IReadOnlyList<IGameTracer>)tracers, null) { }

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification)
    {
        Fanout(x => x.OnNotificationSent(notificationId, recipientIndex, notification), nameof(OnNotificationSent));
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
        Fanout(x => x.OnGameNotificationSent(notificationId, recipientIndex, notification), nameof(OnGameNotificationSent));
    }

    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response)
    {
        Fanout(x => x.OnResponseReceived(notificationId, senderIndex, response), nameof(OnResponseReceived));
    }

    public void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex)
    {
        Fanout(x => x.OnResponseTimeout(notificationId, recipientIndex), nameof(OnResponseTimeout));
    }

    public void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex)
    {
        Fanout(x => x.OnResponseException(notificationId, recipientIndex, ex), nameof(OnResponseException));
    }

    public void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates)
    {
        Fanout(x => x.OnInvalidResponse(notificationId, senderIndex, invalidResponse, presentedCandidates), nameof(OnInvalidResponse));
    }

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
        Fanout(x => x.OnAdoptedAction(phase, adopted), nameof(OnAdoptedAction));
    }

    public void OnRoundStarted(Round round)
    {
        Fanout(x => x.OnRoundStarted(round), nameof(OnRoundStarted));
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        Fanout(x => x.OnRoundEnded(action), nameof(OnRoundEnded));
    }

    private void Fanout(Action<IGameTracer> action, string hook)
    {
        foreach (var tracer in tracers_)
        {
            try
            {
                action(tracer);
            }
            catch (Exception ex)
            {
                logger_?.LogWarning(ex, "GameTracer {Type} が {Hook} で例外を投げました。", tracer.GetType().Name, hook);
                if (tracer.IsCritical)
                {
                    throw;
                }
            }
        }
    }
}
