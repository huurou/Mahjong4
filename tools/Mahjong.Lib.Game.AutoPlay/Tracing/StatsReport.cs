using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// 自動対局の統計集計結果
/// AI 種別 (DisplayName) ごとに集計する。同一 AI が複数席に座った場合もその AI の単一統計にまとめる
/// </summary>
/// <param name="GameCount">対局数</param>
/// <param name="RoundCount">局数 (全対局合計)</param>
/// <param name="PlayerStats">AI 種別ごとの統計 (DisplayName キー)</param>
/// <param name="YakuCounts">役出現回数 (全席合計)</param>
/// <param name="RyuukyokuCounts">流局種別別回数</param>
/// <param name="FailedGameCount">例外で完走できなかった対局数 (AutoPlayRunner が catch して加算)</param>
public record StatsReport(
    int GameCount,
    int RoundCount,
    ImmutableArray<PlayerStats> PlayerStats,
    ImmutableDictionary<string, int> YakuCounts,
    ImmutableDictionary<RyuukyokuType, int> RyuukyokuCounts,
    int FailedGameCount = 0
);
