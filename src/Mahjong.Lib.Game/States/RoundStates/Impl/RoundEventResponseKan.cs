using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 槓応答イベント (暗槓・加槓)
/// </summary>
public record RoundEventResponseKan : RoundEvent
{
    /// <summary>
    /// 槓種別 (Ankan / Kakan)
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// 暗槓: 槓する牌種の牌、加槓: 追加する手牌の牌
    /// </summary>
    public Tile Tile { get; init; }

    public override string Name => "槓応答";

    public RoundEventResponseKan(CallType callType, Tile tile)
    {
        if (callType is not CallType.Ankan and not CallType.Kakan)
        {
            throw new ArgumentException(
                $"槓応答の副露種別は Ankan / Kakan のいずれかである必要があります。実際:{callType}",
                nameof(callType)
            );
        }

        CallType = callType;
        Tile = tile;
    }
}
