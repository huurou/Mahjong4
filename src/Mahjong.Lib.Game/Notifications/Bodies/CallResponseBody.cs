using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications.Bodies;

/// <summary>
/// 副露応答本体
/// </summary>
public record CallResponseBody : ResponseBody
{
    /// <summary>
    /// 副露種別 (Chi / Pon / Daiminkan)
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// 手牌から使う牌
    /// </summary>
    public ImmutableList<Tile> HandTiles { get; init; }

    public CallResponseBody(CallType callType, ImmutableList<Tile> handTiles)
    {
        if (callType is not (CallType.Chi or CallType.Pon or CallType.Daiminkan))
        {
            throw new ArgumentException($"副露応答では Chi / Pon / Daiminkan のみ指定可能です。実際:{callType}", nameof(callType));
        }

        CallType = callType;
        HandTiles = handTiles;
    }

    public virtual bool Equals(CallResponseBody? other)
    {
        return other is not null &&
            CallType == other.CallType &&
            HandTiles.SequenceEqual(other.HandTiles);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(CallType);
        foreach (var tile in HandTiles)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }
}
