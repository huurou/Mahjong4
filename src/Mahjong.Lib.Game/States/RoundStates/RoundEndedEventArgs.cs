using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 局終了イベントの引数基底
/// </summary>
public abstract record RoundEndedEventArgs;

/// <summary>
/// 和了による局終了
/// </summary>
/// <param name="WinnerIndices">和了者 (ダブロン/トリロンなら複数)</param>
/// <param name="LoserIndex">放銃者。ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では <paramref name="WinnerIndices"/>[0] と同値</param>
/// <param name="WinType">和了種別</param>
public record RoundEndedByWinEventArgs(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType
) : RoundEndedEventArgs;

/// <summary>
/// 流局による局終了
/// </summary>
/// <param name="Type">流局種別</param>
/// <param name="TenpaiPlayerIndices">テンパイ者 (荒牌平局時のみ意味を持つ、他は空配列)</param>
/// <param name="NagashiManganPlayerIndices">流し満貫者 (荒牌平局時のみ意味を持つ、他は空配列)</param>
public record RoundEndedByRyuukyokuEventArgs(
    RyuukyokuType Type,
    ImmutableArray<PlayerIndex> TenpaiPlayerIndices,
    ImmutableArray<PlayerIndex> NagashiManganPlayerIndices
) : RoundEndedEventArgs;
