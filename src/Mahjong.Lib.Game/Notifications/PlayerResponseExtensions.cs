using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// C# API 応答型 (PlayerResponse) を Wire DTO 応答本体 (ResponseBody) へ変換する拡張メソッド群
/// ToBody は多対1写像 (TsumoAgariResponse / RonResponse / ChankanRonResponse / RinshanTsumoResponse は同一の WinResponseBody に収束)
/// そのため逆変換には PlayerResponseEnvelopeExtensions.FromWire(RoundInquiryPhase) が必須で
/// phase により同一 Body が異なる C# API 応答型へ復元される
/// 打牌/加槓フェーズのスルーは OkResponse を返し OkResponseBody に変換される
/// </summary>
public static class PlayerResponseExtensions
{
    /// <summary>
    /// C# API 応答を Wire DTO 応答本体に変換する
    /// Envelope 化 (NotificationId / RoundRevision / PlayerIndex 付与) は呼び出し側の責務
    /// 逆変換は FromWire(RoundInquiryPhase) 経由で phase を指定する必要がある
    /// </summary>
    public static ResponseBody ToBody(this PlayerResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response switch
        {
            OkResponse => new OkResponseBody(),
            ChiResponse r => new CallResponseBody(CallType.Chi, [.. r.HandTiles]),
            PonResponse r => new CallResponseBody(CallType.Pon, [.. r.HandTiles]),
            DaiminkanResponse r => new CallResponseBody(CallType.Daiminkan, [.. r.HandTiles]),
            RonResponse => new WinResponseBody(),
            ChankanRonResponse => new WinResponseBody(),
            DahaiResponse r => new DahaiResponseBody(r.Tile, r.IsRiichi),
            AnkanResponse r => new KanResponseBody(CallType.Ankan, r.Tile),
            KakanResponse r => new KanResponseBody(CallType.Kakan, r.Tile),
            TsumoAgariResponse => new WinResponseBody(),
            KyuushuKyuuhaiResponse => new RyuukyokuResponseBody(),
            RinshanTsumoResponse => new WinResponseBody(),
            KanTsumoDahaiResponse r => new DahaiResponseBody(r.Tile, r.IsRiichi),
            KanTsumoAnkanResponse r => new KanResponseBody(CallType.Ankan, r.Tile),
            KanTsumoKakanResponse r => new KanResponseBody(CallType.Kakan, r.Tile),
            _ => throw new ArgumentException($"未対応の応答型です。実際:{response.GetType().Name}", nameof(response)),
        };
    }
}
