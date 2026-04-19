using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
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
/// <param name="Winners">和了者毎の明細 (Index / 和了牌 / スコア計算結果)。
/// 既存テスト互換のため null 可、RoundStateContext 直接駆動経路では null、RoundManager 経由では常に付与される</param>
/// <param name="Honba">精算前の本場</param>
/// <param name="KyoutakuRiichiAward">供託立直棒の受取情報 (供託がない場合は null)</param>
public record RoundEndedByWinEventArgs(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType,
    ImmutableArray<AdoptedWinner> Winners = default,
    Honba? Honba = null,
    KyoutakuRiichiAward? KyoutakuRiichiAward = null
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
