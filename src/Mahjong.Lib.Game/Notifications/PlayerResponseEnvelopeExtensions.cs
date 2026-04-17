using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// Wire DTO 応答エンベロープ (PlayerResponseEnvelope) を C# API 応答型 (PlayerResponse) へ変換する拡張メソッド群
/// </summary>
public static class PlayerResponseEnvelopeExtensions
{
    /// <summary>
    /// Wire DTO 応答エンベロープを C# API 応答型に変換する
    /// phase によって応答型階層 (AfterTsumo / AfterDahai / AfterKan / AfterKanTsumo / Ok) が決定される
    /// </summary>
    public static PlayerResponse FromWire(this PlayerResponseEnvelope envelope, RoundDecisionPhase phase)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(envelope.Body);

        var body = envelope.Body;
        return phase switch
        {
            RoundDecisionPhase.Haipai => FromWireHaipai(body),
            RoundDecisionPhase.Tsumo => FromWireTsumo(body),
            RoundDecisionPhase.Dahai => FromWireDahai(body),
            RoundDecisionPhase.Kan => FromWireKan(body),
            RoundDecisionPhase.KanTsumo => FromWireKanTsumo(body),
            RoundDecisionPhase.AfterKanTsumo => FromWireAfterKanTsumo(body),
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "未対応のフェーズです。"),
        };
    }

    private static OkResponse FromWireHaipai(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new OkResponse(),
            _ => throw InvalidBody(RoundDecisionPhase.Haipai, body),
        };
    }

    private static PlayerResponse FromWireTsumo(ResponseBody body)
    {
        return body switch
        {
            DahaiResponseBody b => new DahaiResponse(b.Tile, b.IsRiichi),
            KanResponseBody b when b.CallType == CallType.Ankan => new AnkanResponse(b.Tile),
            KanResponseBody b when b.CallType == CallType.Kakan => new KakanResponse(b.Tile),
            WinResponseBody => new TsumoAgariResponse(),
            RyuukyokuResponseBody => new KyuushuKyuuhaiResponse(),
            _ => throw InvalidBody(RoundDecisionPhase.Tsumo, body),
        };
    }

    private static PlayerResponse FromWireDahai(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new PassResponse(),
            CallResponseBody b when b.CallType == CallType.Chi => new ChiResponse([.. b.HandTiles]),
            CallResponseBody b when b.CallType == CallType.Pon => new PonResponse([.. b.HandTiles]),
            CallResponseBody b when b.CallType == CallType.Daiminkan => new DaiminkanResponse([.. b.HandTiles]),
            WinResponseBody => new RonResponse(),
            _ => throw InvalidBody(RoundDecisionPhase.Dahai, body),
        };
    }

    private static PlayerResponse FromWireKan(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new KanPassResponse(),
            WinResponseBody => new ChankanRonResponse(),
            _ => throw InvalidBody(RoundDecisionPhase.Kan, body),
        };
    }

    private static PlayerResponse FromWireKanTsumo(ResponseBody body)
    {
        return body switch
        {
            DahaiResponseBody b => new KanTsumoDahaiResponse(b.Tile, b.IsRiichi),
            KanResponseBody b when b.CallType == CallType.Ankan => new KanTsumoAnkanResponse(b.Tile),
            KanResponseBody b when b.CallType == CallType.Kakan => new KanTsumoKakanResponse(b.Tile),
            WinResponseBody => new RinshanTsumoResponse(),
            _ => throw InvalidBody(RoundDecisionPhase.KanTsumo, body),
        };
    }

    private static PlayerResponse FromWireAfterKanTsumo(ResponseBody body)
    {
        return body switch
        {
            DahaiResponseBody b => new KanTsumoDahaiResponse(b.Tile, b.IsRiichi),
            KanResponseBody b when b.CallType == CallType.Ankan => new KanTsumoAnkanResponse(b.Tile),
            KanResponseBody b when b.CallType == CallType.Kakan => new KanTsumoKakanResponse(b.Tile),
            _ => throw InvalidBody(RoundDecisionPhase.AfterKanTsumo, body),
        };
    }

    private static ArgumentException InvalidBody(RoundDecisionPhase phase, ResponseBody body)
    {
        return new ArgumentException($"フェーズ {phase} では envelope.Body ({body.GetType().Name}) は許可されていません。", nameof(body));
    }
}
