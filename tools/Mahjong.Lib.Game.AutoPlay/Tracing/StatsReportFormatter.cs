using System.Text;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// StatsReport をコンソール出力用にフォーマットする
/// </summary>
public static class StatsReportFormatter
{
    public static string Format(StatsReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Stats (Games: {report.GameCount} Rounds: {report.RoundCount}) ===");
        if (report.FailedGameCount > 0)
        {
            sb.AppendLine($"!! 例外で失敗した対局: {report.FailedGameCount}");
        }

        sb.AppendLine();
        sb.AppendLine("AI種別別統計:");
        var nameWidth = Math.Max(20, report.PlayerStats.Select(x => x.DisplayName.Length).DefaultIfEmpty(20).Max());
        sb.AppendLine($"{"AI".PadRight(nameWidth)} | 席局 |  局  | 1位 | 2位 | 3位 | 4位 | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点");
        foreach (var stats in report.PlayerStats)
        {
            sb.AppendLine(
                $"{stats.DisplayName.PadRight(nameWidth)}" +
                $" | {stats.GameSeatCount,4}" +
                $" | {stats.RoundAppearance,4}" +
                $" | {stats.RankCounts[0],3}" +
                $" | {stats.RankCounts[1],3}" +
                $" | {stats.RankCounts[2],3}" +
                $" | {stats.RankCounts[3],3}" +
                $" | {stats.AverageRank,8:F3}" +
                $" | {stats.WinRate,6:P1}" +
                $" | {stats.HoujuuRate,6:P1}" +
                $" | {stats.RiichiRate,6:P1}" +
                $" | {stats.CallRate,6:P1}" +
                $" | {stats.AverageWinPoint,8:F0}"
            );
        }

        sb.AppendLine();
        var totalWins = report.PlayerStats.Sum(x => x.WinCount);
        sb.AppendLine($"役出現回数 (上位 20、和了回数 {totalWins} 回に対する出現率):");
        foreach (var kv in report.YakuCounts.OrderByDescending(x => x.Value).Take(20))
        {
            var rate = totalWins == 0 ? 0.0 : (double)kv.Value / totalWins;
            sb.AppendLine($"  {kv.Key,-20} {kv.Value,5} ({rate,6:P1})");
        }

        sb.AppendLine();
        sb.AppendLine("流局種別別回数:");
        foreach (var kv in report.RyuukyokuCounts.OrderByDescending(x => x.Value))
        {
            sb.AppendLine($"  {kv.Key,-20} {kv.Value}");
        }

        return sb.ToString();
    }
}
