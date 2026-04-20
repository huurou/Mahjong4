using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Scoring.Games;
using GameRules = Mahjong.Lib.Game.Games.GameRules;

namespace Mahjong.Lib.Game.Scoring.Conversions;

internal static class WinSituationConverter
{
    public static WinSituation ToWinSituation(ScoreRequest request, GameRules rules)
    {
        var round = request.Round;
        var winnerIndex = request.WinnerIndex;
        var status = round.PlayerRoundStatusArray[winnerIndex];
        var dealerIndex = round.RoundNumber.ToDealer();

        var isTsumo = request.WinType is WinType.Tsumo or WinType.Rinshan;
        var isFirstTurn = status.IsFirstTurnBeforeDiscard;
        var isLiveExhausted = round.Wall.LiveRemaining == 0;
        var akadoraCount = CountAkadora(round, winnerIndex, rules);

        return new WinSituation
        {
            IsTsumo = isTsumo,
            IsRiichi = status.IsRiichi,
            IsDoubleRiichi = status.IsDoubleRiichi,
            IsIppatsu = status.IsIppatsu,
            IsRinshan = request.WinType == WinType.Rinshan,
            IsChankan = request.WinType == WinType.Chankan,
            IsHaitei = request.WinType == WinType.Tsumo && isLiveExhausted,
            IsHoutei = request.WinType == WinType.Ron && isLiveExhausted,
            IsTenhou = request.WinType == WinType.Tsumo && isFirstTurn && winnerIndex == dealerIndex,
            IsChiihou = request.WinType == WinType.Tsumo && isFirstTurn && winnerIndex != dealerIndex,
            // 人和: 子の第一打前のロン。鳴きが入ると PlayerRoundStatus.IsFirstTurnBeforeDiscard が全員 false になるので本条件だけで十分
            IsRenhou = request.WinType == WinType.Ron && isFirstTurn && winnerIndex != dealerIndex,
            // 流し満貫は Round.SettleRyuukyoku 側で点数清算まで完結する設計のため、和了パスを通る本コンバーターでは常に false で良い
            IsNagashimangan = false,
            PlayerWind = ToWind(SeatWindOf(winnerIndex, dealerIndex)),
            RoundWind = ToWind(round.RoundWind.Value),
            AkadoraCount = akadoraCount,
        };
    }

    private static int SeatWindOf(PlayerIndex playerIndex, PlayerIndex dealerIndex)
    {
        return (playerIndex.Value - dealerIndex.Value + PlayerIndex.PLAYER_COUNT) % PlayerIndex.PLAYER_COUNT;
    }

    private static Wind ToWind(int value)
    {
        return value switch
        {
            0 => Wind.East,
            1 => Wind.South,
            2 => Wind.West,
            3 => Wind.North,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "風の値は 0-3 の範囲である必要があります。"),
        };
    }

    private static int CountAkadora(Round round, PlayerIndex winnerIndex, GameRules rules)
    {
        var handReds = round.HandArray[winnerIndex].Count(rules.IsRedDora);
        var callReds = round.CallListArray[winnerIndex].Sum(x => x.Tiles.Count(rules.IsRedDora));
        return handReds + callReds;
    }
}
