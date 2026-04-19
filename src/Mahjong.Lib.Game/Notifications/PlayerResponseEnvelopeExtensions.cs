using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
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
    public static PlayerResponse FromWire(this PlayerResponseEnvelope envelope, RoundInquiryPhase phase)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(envelope.Body);

        var body = envelope.Body;
        return phase switch
        {
            RoundInquiryPhase.Haipai => FromWireHaipai(body),
            RoundInquiryPhase.Tsumo => FromWireTsumo(body),
            RoundInquiryPhase.Dahai => FromWireDahai(body),
            RoundInquiryPhase.Kan => FromWireKan(body),
            RoundInquiryPhase.KanTsumo => FromWireKanTsumo(body),
            RoundInquiryPhase.AfterKanTsumo => FromWireAfterKanTsumo(body),
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "未対応のフェーズです。"),
        };
    }

    private static OkResponse FromWireHaipai(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new OkResponse(),
            _ => throw InvalidBody(RoundInquiryPhase.Haipai, body),
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
            _ => throw InvalidBody(RoundInquiryPhase.Tsumo, body),
        };
    }

    private static PlayerResponse FromWireDahai(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new OkResponse(),
            CallResponseBody b when b.CallType == CallType.Chi => new ChiResponse([.. b.HandTiles]),
            CallResponseBody b when b.CallType == CallType.Pon => new PonResponse([.. b.HandTiles]),
            CallResponseBody b when b.CallType == CallType.Daiminkan => new DaiminkanResponse([.. b.HandTiles]),
            WinResponseBody => new RonResponse(),
            _ => throw InvalidBody(RoundInquiryPhase.Dahai, body),
        };
    }

    private static PlayerResponse FromWireKan(ResponseBody body)
    {
        return body switch
        {
            OkResponseBody => new OkResponse(),
            WinResponseBody => new ChankanRonResponse(),
            _ => throw InvalidBody(RoundInquiryPhase.Kan, body),
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
            _ => throw InvalidBody(RoundInquiryPhase.KanTsumo, body),
        };
    }

    private static PlayerResponse FromWireAfterKanTsumo(ResponseBody body)
    {
        return body switch
        {
            DahaiResponseBody b => new KanTsumoDahaiResponse(b.Tile, b.IsRiichi),
            KanResponseBody b when b.CallType == CallType.Ankan => new KanTsumoAnkanResponse(b.Tile),
            KanResponseBody b when b.CallType == CallType.Kakan => new KanTsumoKakanResponse(b.Tile),
            _ => throw InvalidBody(RoundInquiryPhase.AfterKanTsumo, body),
        };
    }

    private static ArgumentException InvalidBody(RoundInquiryPhase phase, ResponseBody body)
    {
        return new ArgumentException($"フェーズ {phase} では envelope.Body ({body.GetType().Name}) は許可されていません。", nameof(body));
    }

    /// <summary>
    /// <see cref="GameNotification"/> および局内 OK 応答通知 (行動選択を伴わない通知) への Wire ACK を
    /// C# API 応答型 (OkResponse) に変換する。
    /// 呼び出し側は対象通知が OK 応答のみ受理する種別であることを保証する責務を持つ
    /// (Tsumo / Dahai / Kan / KanTsumo などには FromWire(RoundInquiryPhase) を使う)。
    /// </summary>
    public static OkResponse FromWireOk(this PlayerResponseEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(envelope.Body);

        return envelope.Body switch
        {
            OkResponseBody => new OkResponse(),
            _ => throw new ArgumentException(
                $"OK 応答通知への Wire ACK は OkResponseBody のみ受理可能です。実際:{envelope.Body.GetType().Name}",
                nameof(envelope)
            ),
        };
    }
}
