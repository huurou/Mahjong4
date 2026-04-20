using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Scoring.Conversions;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using GameCallType = Mahjong.Lib.Game.Calls.CallType;

namespace Mahjong.Lib.Game.Scoring;

/// <summary>
/// Mahjong.Lib.Scoring をラップした IScoreCalculator の本実装
/// </summary>
public sealed class ScoreCalculatorImpl(GameRules rules) : IScoreCalculator
{
    public ScoreResult Calculate(ScoreRequest request)
    {
        var round = request.Round;
        var winnerIndex = request.WinnerIndex;
        var hand = round.HandArray[winnerIndex];
        var callList = round.CallListArray[winnerIndex];

        // Lib.Scoring の HandCalculator は 14 枚相当の手牌 TileKindList を期待する (副露は callList 経由)。
        // Tsumo/Rinshan: 手牌は既にツモ牌を含む 14 枚相当 → そのまま渡す
        // Ron/Chankan: 手牌 13 枚 → winTile を追加して 14 枚にする
        var tileKindList = hand.Select(x => TileKindConverter.FromKind(x.Kind)).ToList();
        var isSelfDraw = request.WinType is WinType.Tsumo or WinType.Rinshan;
        if (!isSelfDraw)
        {
            tileKindList.Add(request.WinTile.ToTileKind());
        }
        var handTileKindList = new TileKindList(tileKindList);

        var winTileKind = request.WinTile.ToTileKind();
        var scoringCallList = callList.ToScoringCallList();
        var doraIndicators = CollectDoraIndicators(round);
        var isRiichi = round.PlayerRoundStatusArray[winnerIndex].IsRiichi ||
            round.PlayerRoundStatusArray[winnerIndex].IsDoubleRiichi;
        var uradoraIndicators = isRiichi ? CollectUradoraIndicators(round) : [];
        var winSituation = WinSituationConverter.ToWinSituation(request, rules);
        var scoringRules = rules.ToScoringGameRules();

        var result = HandCalculator.Calc(
            handTileKindList,
            winTileKind,
            scoringCallList,
            doraIndicators,
            uradoraIndicators,
            winSituation,
            scoringRules
        );

        var isOpen = callList.Any(x => x.Type is not GameCallType.Ankan);
        return HandResultConverter.ToScoreResult(result, request, isOpen);
    }

    private static TileKindList CollectDoraIndicators(Rounds.Round round)
    {
        var wall = round.Wall;
        var builder = new List<TileKind>(wall.DoraRevealedCount);
        for (var n = 0; n < wall.DoraRevealedCount; n++)
        {
            builder.Add(wall.GetDoraIndicator(n).ToTileKind());
        }
        return [.. builder];
    }

    private static TileKindList CollectUradoraIndicators(Rounds.Round round)
    {
        var wall = round.Wall;
        var builder = new List<TileKind>(wall.DoraRevealedCount);
        for (var n = 0; n < wall.DoraRevealedCount; n++)
        {
            builder.Add(wall.GetUradoraIndicator(n).ToTileKind());
        }
        return [.. builder];
    }
}
