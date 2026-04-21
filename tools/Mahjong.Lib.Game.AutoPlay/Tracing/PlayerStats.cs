using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// AI種別 (DisplayName) ごとの統計。同一 AI が複数席に座っている場合も単一エントリに集約される
/// </summary>
/// <param name="DisplayName">AI の表示名</param>
/// <param name="RankCounts">順位別カウント (index 0=1位)。同一対局で 2 席に居れば 2 エントリ分加算</param>
/// <param name="GameSeatCount">席数×対局数 (AverageRank・順位率の分母)</param>
/// <param name="RoundAppearance">席数×局数 (和了率・放銃率・立直率・副露率の分母)</param>
/// <param name="WinCount">和了回数 (全席合計)</param>
/// <param name="HoujuuCount">放銃回数 (全席合計)</param>
/// <param name="RiichiCount">立直回数 (局×席単位、1局1席につき1回まで)</param>
/// <param name="CallCount">副露局数 (局×席単位、1局1席につき1回まで)</param>
/// <param name="WinPointSum">和了点数合計 (全席合計)</param>
public record PlayerStats(
    string DisplayName,
    ImmutableArray<int> RankCounts,
    int GameSeatCount,
    int RoundAppearance,
    int WinCount,
    int HoujuuCount,
    int RiichiCount,
    int CallCount,
    long WinPointSum
)
{
    public double AverageRank
    {
        get
        {
            if (GameSeatCount == 0) { return 0; }
            var weightedSum = 0;
            for (var rank = 0; rank < RankCounts.Length; rank++)
            {
                weightedSum += RankCounts[rank] * (rank + 1);
            }
            return (double)weightedSum / GameSeatCount;
        }
    }

    public double WinRate => RoundAppearance == 0 ? 0 : (double)WinCount / RoundAppearance;

    public double HoujuuRate => RoundAppearance == 0 ? 0 : (double)HoujuuCount / RoundAppearance;

    public double RiichiRate => RoundAppearance == 0 ? 0 : (double)RiichiCount / RoundAppearance;

    public double CallRate => RoundAppearance == 0 ? 0 : (double)CallCount / RoundAppearance;

    public double AverageWinPoint => WinCount == 0 ? 0 : (double)WinPointSum / WinCount;
}
