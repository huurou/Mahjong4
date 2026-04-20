using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// IGameTracer として対局イベントを JSONL 牌譜として出力する
/// </summary>
public sealed class PaifuRecorder(JsonlPaifuWriter writer) : IGameTracer, IDisposable
{
    private readonly JsonlPaifuWriter writer_ = writer;

    /// <summary>
    /// 牌譜の IO 失敗は後続統計の整合性を壊すため、<see cref="CompositeGameTracer"/> で例外を再 throw させる
    /// </summary>
    public bool IsCritical => true;

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification)
    {
        writer_.Write(new PaifuEntry("notify", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("to", recipientIndex.Value)
            .Add("notification", notification.GetType().Name)));
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
        writer_.Write(new PaifuEntry("game-notify", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("to", recipientIndex.Value)
            .Add("notification", notification.GetType().Name)));
    }

    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response)
    {
        writer_.Write(new PaifuEntry("response", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("from", senderIndex.Value)
            .Add("response", response.GetType().Name)));
    }

    public void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex)
    {
        writer_.Write(new PaifuEntry("timeout", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("to", recipientIndex.Value)));
    }

    public void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex)
    {
        writer_.Write(new PaifuEntry("exception", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("to", recipientIndex.Value)
            .Add("message", ex.Message)));
    }

    public void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates)
    {
        writer_.Write(new PaifuEntry("invalid-response", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("from", senderIndex.Value)
            .Add("response", invalidResponse.GetType().Name)));
    }

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
        writer_.Write(new PaifuEntry("adopted", ImmutableDictionary<string, object?>.Empty
            .Add("phase", phase.ToString())
            .Add("from", adopted.PlayerIndex.Value)
            .Add("response", adopted.Response.GetType().Name)));
    }

    public void OnRoundStarted(Round round)
    {
        writer_.Write(new PaifuEntry("round-start", ImmutableDictionary<string, object?>.Empty
            .Add("roundWind", round.RoundWind.Value)
            .Add("roundNumber", round.RoundNumber.Value)
            .Add("honba", round.Honba.Value)
            .Add("kyoutakuRiichiCount", round.KyoutakuRiichiCount.Value)));
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        writer_.Write(new PaifuEntry("round-end", ImmutableDictionary<string, object?>.Empty
            .Add("action", action.GetType().Name)));
    }

    public void Dispose()
    {
        writer_.Dispose();
    }
}
