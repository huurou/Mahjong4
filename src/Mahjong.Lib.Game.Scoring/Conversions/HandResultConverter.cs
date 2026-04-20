using System.Collections.Immutable;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Yakus;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Game.Scoring.Conversions;

internal static class HandResultConverter
{
    private const int YAKUMAN_HAN = 13;

    public static ScoreResult ToScoreResult(HandResult result, ScoreRequest request, bool isOpen)
    {
        if (result.ErrorMessage is not null)
        {
            throw new InvalidOperationException($"HandCalculator がエラーを返しました: {result.ErrorMessage}");
        }

        var deltas = DistributePointDeltas(result, request);
        var yakuInfos = ToYakuInfoList(result.YakuList, isOpen);
        return new ScoreResult(result.Han, result.Fu, deltas, yakuInfos);
    }

    private static PointArray DistributePointDeltas(HandResult result, ScoreRequest request)
    {
        var round = request.Round;
        var winnerIndex = request.WinnerIndex;
        var loserIndex = request.LoserIndex;
        var winType = request.WinType;
        var dealerIndex = round.RoundNumber.ToDealer();
        var main = result.Score.Main;
        var sub = result.Score.Sub;

        var deltas = new PointArray(new Point(0));

        if (winType is WinType.Ron or WinType.Chankan)
        {
            deltas = deltas.AddPoint(winnerIndex, main).SubtractPoint(loserIndex, main);
            return deltas;
        }

        var isDealerWin = winnerIndex == dealerIndex;
        if (isDealerWin)
        {
            var totalWinnerGain = 0;
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var playerIndex = new PlayerIndex(i);
                if (playerIndex == winnerIndex) { continue; }
                deltas = deltas.SubtractPoint(playerIndex, main);
                totalWinnerGain += main;
            }
            deltas = deltas.AddPoint(winnerIndex, totalWinnerGain);
            return deltas;
        }

        var totalGain = 0;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            if (playerIndex == winnerIndex) { continue; }
            var amount = playerIndex == dealerIndex ? main : sub;
            deltas = deltas.SubtractPoint(playerIndex, amount);
            totalGain += amount;
        }
        deltas = deltas.AddPoint(winnerIndex, totalGain);
        return deltas;
    }

    private static ImmutableList<YakuInfo> ToYakuInfoList(YakuList yakuList, bool isOpen)
    {
        var builder = ImmutableList.CreateBuilder<YakuInfo>();
        foreach (var yaku in yakuList)
        {
            builder.Add(ToYakuInfo(yaku, isOpen));
        }
        return builder.ToImmutable();
    }

    private static YakuInfo ToYakuInfo(Yaku yaku, bool isOpen)
    {
        var isYakuman = yaku.IsYakuman;
        var han = isOpen ? yaku.HanOpen : yaku.HanClosed;
        var yakumanCount = isYakuman ? Math.Max(1, han / YAKUMAN_HAN) : 0;
        int? displayHan = isYakuman ? null : han;
        return new YakuInfo(
            yaku.Number,
            yaku.Name,
            displayHan,
            yakumanCount,
            IsPaoEligible(yaku)
        );
    }

    private static bool IsPaoEligible(Yaku yaku)
    {
        return yaku is Daisangen or Daisuushii or DaisuushiiDouble or Suukantsu;
    }
}
