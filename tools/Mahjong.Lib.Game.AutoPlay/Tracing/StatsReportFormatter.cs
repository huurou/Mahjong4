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
        sb.AppendLine("プレイヤー別統計:");
        sb.AppendLine("Idx | 1位 | 2位 | 3位 | 4位 | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点");
        for (var i = 0; i < 4; i++)
        {
            var counts = report.RankCounts[i];
            sb.AppendLine(
                $"  {i} | {counts[0],3} | {counts[1],3} | {counts[2],3} | {counts[3],3}" +
                $" | {report.AverageRank(i),8:F3}" +
                $" | {report.WinRate(i),6:P1}" +
                $" | {report.HoujuuRate(i),6:P1}" +
                $" | {report.RiichiRate(i),6:P1}" +
                $" | {report.CallRate(i),6:P1}" +
                $" | {report.AverageWinPoint(i),8:F0}"
            );
        }

        sb.AppendLine();
        sb.AppendLine("役出現回数 (上位 20):");
        foreach (var kv in report.YakuCounts.OrderByDescending(x => x.Value).Take(20))
        {
            sb.AppendLine($"  {kv.Key,-20} {kv.Value}");
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
