using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Bodies;

/// <summary>
/// 槓応答本体
/// </summary>
public record KanResponseBody : ResponseBody
{
    /// <summary>
    /// 槓種別 (Ankan / Kakan)
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// 暗槓: 槓する牌種の牌、加槓: 追加する手牌の牌
    /// </summary>
    public Tile Tile { get; init; }

    public KanResponseBody(CallType callType, Tile tile)
    {
        if (callType is not (CallType.Ankan or CallType.Kakan))
        {
            throw new ArgumentException($"槓応答では Ankan / Kakan のみ指定可能です。実際:{callType}", nameof(callType));
        }

        CallType = callType;
        Tile = tile;
    }
}
