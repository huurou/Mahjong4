using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// IGameTracer として対局イベントを JSONL 牌譜として出力する。
/// 通知・応答・採用アクションは具象型のまま JSON シリアライズして保存するので、
/// 同じシード・同じプレイヤー実装なら牌譜からリプレイ可能な情報量を持つ
/// </summary>
public sealed class PaifuRecorder(JsonlPaifuWriter writer) : IGameTracer, IDisposable
{
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

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
            .Add("type", notification.GetType().Name)
            .Add("payload", Serialize(notification))));
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
        writer_.Write(new PaifuEntry("game-notify", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("to", recipientIndex.Value)
            .Add("type", notification.GetType().Name)
            .Add("payload", Serialize(notification))));
    }

    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response)
    {
        writer_.Write(new PaifuEntry("response", ImmutableDictionary<string, object?>.Empty
            .Add("id", notificationId.Value)
            .Add("from", senderIndex.Value)
            .Add("type", response.GetType().Name)
            .Add("payload", Serialize(response))));
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
            .Add("type", invalidResponse.GetType().Name)
            .Add("payload", Serialize(invalidResponse))));
    }

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
        writer_.Write(new PaifuEntry("adopted", ImmutableDictionary<string, object?>.Empty
            .Add("phase", phase.ToString())
            .Add("from", adopted.PlayerIndex.Value)
            .Add("type", adopted.Response.GetType().Name)
            .Add("payload", Serialize(adopted.Response))));
    }

    public void OnRoundStarted(Round round)
    {
        writer_.Write(new PaifuEntry("round-start", ImmutableDictionary<string, object?>.Empty
            .Add("payload", Serialize(round))));
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        writer_.Write(new PaifuEntry("round-end", ImmutableDictionary<string, object?>.Empty
            .Add("type", action.GetType().Name)
            .Add("payload", Serialize(action))));
    }

    private static JsonElement Serialize(object value)
    {
        return JsonSerializer.SerializeToElement(value, value.GetType(), JsonOptions);
    }

    public void Dispose()
    {
        writer_.Dispose();
    }
}
