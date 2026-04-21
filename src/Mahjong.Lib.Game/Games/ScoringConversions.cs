using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Game.Games;

internal static class ScoringConversions
{
    public static TileKindList ToScoringTileKindList(this Hands.Hand hand)
    {
        return [.. hand.Select(x => x.Kind)];
    }

    public static TileKindList ToScoringTileKindList(this IEnumerable<Tile> tiles)
    {
        return [.. tiles.Select(x => x.Kind)];
    }

    public static Scoring.Calls.Call ToScoringCall(this Call call)
    {
        var type = call.Type switch
        {
            CallType.Chi => Scoring.Calls.CallType.Chi,
            CallType.Pon => Scoring.Calls.CallType.Pon,
            CallType.Ankan => Scoring.Calls.CallType.Ankan,
            CallType.Daiminkan => Scoring.Calls.CallType.Minkan,
            CallType.Kakan => Scoring.Calls.CallType.Minkan,
            _ => throw new ArgumentOutOfRangeException(nameof(call), call.Type, "未対応の副露種別です。"),
        };
        return new Scoring.Calls.Call(type, call.Tiles.ToScoringTileKindList());
    }

    public static Scoring.Calls.CallList ToScoringCallList(this CallList callList)
    {
        return [.. callList.Select(ToScoringCall)];
    }

    public static Scoring.Games.GameRules ToScoringGameRules(this GameRules rules)
    {
        return new Scoring.Games.GameRules
        {
            KuitanEnabled = rules.KuitanAllowed,
        };
    }

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
            IsRenhou = request.WinType == WinType.Ron && isFirstTurn && winnerIndex != dealerIndex,
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
