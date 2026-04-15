using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 副露応答イベント (チー・ポン・大明槓)
/// </summary>
public record RoundEventResponseCall : RoundEvent
{
    /// <summary>
    /// 副露するプレイヤー
    /// </summary>
    public PlayerIndex Caller { get; init; }

    /// <summary>
    /// 副露種別 (Chi / Pon / Daiminkan)
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// callerの手牌から使用する牌 (Chi・Pon: 2枚、Daiminkan: 3枚)
    /// </summary>
    public ImmutableList<Tile> HandTiles { get; init; }

    /// <summary>
    /// 鳴かれる牌 (直前の打牌)
    /// </summary>
    public Tile CalledTile { get; init; }

    public override string Name => "副露応答";

    public RoundEventResponseCall(PlayerIndex caller, CallType callType, ImmutableList<Tile> handTiles, Tile calledTile)
    {
        if (callType is not CallType.Chi and not CallType.Pon and not CallType.Daiminkan)
        {
            throw new ArgumentException(
                $"副露応答の副露種別は Chi / Pon / Daiminkan のいずれかである必要があります。実際:{callType}",
                nameof(callType)
            );
        }
        Call.Validate(callType, handTiles.Add(calledTile));

        Caller = caller;
        CallType = callType;
        HandTiles = handTiles;
        CalledTile = calledTile;
    }
}
