using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Fus;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating.Scores;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.HandCalculating;

/// <summary>
/// 手牌の計算結果を表現するクラス
/// </summary>
/// <param name="Fu">符</param>
/// <param name="Han">翻</param>
/// <param name="Score">点数</param>
/// <param name="YakuList">役のリスト</param>
/// <param name="FuList">符のリスト</param>
/// <param name="ErrorMessage">エラーメッセージ</param>
public record HandResult(int Fu, int Han, Score Score, YakuList YakuList, FuList FuList, string? ErrorMessage)
{
    public static HandResult Create(YakuList yakuList, FuList? fuList = null, CallList? callList = null, WinSituation? winSituation = null, GameRules? gameRules = null)
    {
        fuList ??= [];
        callList ??= [];
        winSituation ??= new();
        gameRules ??= new();

        var fu = fuList.Total;
        var han = callList.HasOpen ? yakuList.Sum(x => x.HanOpen) : yakuList.Sum(x => x.HanClosed);
        if (han == 0)
        {
            return Error("役がありません。");
        }

        var isYakuman = yakuList.Any(x => x.IsYakuman);
        var score = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman);
        return new(fu, han, score, [.. yakuList], fuList, null);
    }

    public static HandResult Error(string message)
    {
        return new(0, 0, new(0), [], [], message);
    }
}
