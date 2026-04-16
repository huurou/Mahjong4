using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 副露 (チー/ポン/大明槓) 採用結果
/// </summary>
/// <param name="CallerIndex">副露するプレイヤー</param>
/// <param name="CallType">Chi / Pon / Daiminkan</param>
/// <param name="HandTiles">手牌から使う牌</param>
public record ResolvedCallAction : ResolvedRoundAction
{
    public PlayerIndex CallerIndex { get; init; }
    public CallType CallType { get; init; }
    public ImmutableList<Tile> HandTiles { get; init; }

    public ResolvedCallAction(PlayerIndex callerIndex, CallType callType, ImmutableList<Tile> handTiles)
    {
        if (callType is not CallType.Chi and not CallType.Pon and not CallType.Daiminkan)
        {
            throw new ArgumentException($"副露採用結果では Chi / Pon / Daiminkan のみ指定可能です。実際:{callType}", nameof(callType));
        }

        CallerIndex = callerIndex;
        CallType = callType;
        HandTiles = handTiles;
    }

    public virtual bool Equals(ResolvedCallAction? other)
    {
        return other is not null &&
            CallerIndex == other.CallerIndex &&
            CallType == other.CallType &&
            HandTiles.SequenceEqual(other.HandTiles);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(CallerIndex);
        hash.Add(CallType);
        foreach (var tile in HandTiles)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }
}
