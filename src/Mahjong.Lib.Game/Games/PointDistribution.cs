using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Scoring.HandCalculating;

namespace Mahjong.Lib.Game.Games;

internal static class PointDistribution
{
    public static PointArray Distribute(HandResult result, ScoreRequest request)
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
            return deltas.AddPoint(winnerIndex, main).SubtractPoint(loserIndex, main);
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
            return deltas.AddPoint(winnerIndex, totalWinnerGain);
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
        return deltas.AddPoint(winnerIndex, totalGain);
    }
}
