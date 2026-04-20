using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// 自動対局の統計集計結果 (4 プレイヤー分)
/// </summary>
/// <param name="GameCount">対局数</param>
/// <param name="RoundCount">局数 (全対局合計)</param>
/// <param name="RankCounts">プレイヤー別 順位別カウント ([player][rank] rank=0 が 1 位)</param>
/// <param name="WinCounts">プレイヤー別 和了回数</param>
/// <param name="HoujuuCounts">プレイヤー別 放銃回数</param>
/// <param name="RiichiCounts">プレイヤー別 立直回数 (1 局 1 回まで)</param>
/// <param name="CallCounts">プレイヤー別 副露局数 (1 局 1 回まで)</param>
/// <param name="WinPointSums">プレイヤー別 和了点数合計</param>
/// <param name="YakuCounts">役出現回数</param>
/// <param name="RyuukyokuCounts">流局種別別回数</param>
/// <param name="FailedGameCount">例外で完走できなかった対局数 (AutoPlayRunner が catch して加算)</param>
public record StatsReport(
    int GameCount,
    int RoundCount,
    ImmutableArray<ImmutableArray<int>> RankCounts,
    ImmutableArray<int> WinCounts,
    ImmutableArray<int> HoujuuCounts,
    ImmutableArray<int> RiichiCounts,
    ImmutableArray<int> CallCounts,
    ImmutableArray<long> WinPointSums,
    ImmutableDictionary<string, int> YakuCounts,
    ImmutableDictionary<RyuukyokuType, int> RyuukyokuCounts,
    int FailedGameCount = 0
)
{
    /// <summary>
    /// プレイヤーindex i の 平均順位 (1-4)
    /// </summary>
    public double AverageRank(int playerIndex)
    {
        var counts = RankCounts[playerIndex];
        var totalGames = counts.Sum();
        if (totalGames == 0) { return 0; }
        var weightedSum = 0;
        for (var rank = 0; rank < counts.Length; rank++)
        {
            weightedSum += counts[rank] * (rank + 1);
        }
        return (double)weightedSum / totalGames;
    }

    public double WinRate(int playerIndex)
    {
        return RoundCount == 0 ? 0 : (double)WinCounts[playerIndex] / RoundCount;
    }

    public double HoujuuRate(int playerIndex)
    {
        return RoundCount == 0 ? 0 : (double)HoujuuCounts[playerIndex] / RoundCount;
    }

    public double RiichiRate(int playerIndex)
    {
        return RoundCount == 0 ? 0 : (double)RiichiCounts[playerIndex] / RoundCount;
    }

    public double CallRate(int playerIndex)
    {
        return RoundCount == 0 ? 0 : (double)CallCounts[playerIndex] / RoundCount;
    }

    public double AverageWinPoint(int playerIndex)
    {
        return WinCounts[playerIndex] == 0 ? 0 : (double)WinPointSums[playerIndex] / WinCounts[playerIndex];
    }
}
