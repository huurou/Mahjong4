using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 和了時点数計算のエントリポイント。
/// Lib.Scoring の <see cref="HandCalculator"/> をラップし、Lib.Game 側の型 (Round/Tile/Call) を受け取って
/// 親子・ツモロン分配を含む <see cref="ScoreResult"/> を返します
/// </summary>
internal static class ScoringHelper
{
    public static ScoreResult Calculate(ScoreRequest request, GameRules rules)
    {
        var round = request.Round;
        var winnerIndex = request.WinnerIndex;
        var hand = round.HandArray[winnerIndex];
        var callList = round.CallListArray[winnerIndex];

        // Lib.Scoring の HandCalculator は 14 枚相当の手牌 TileKindList を期待する (副露は callList 経由)。
        // Tsumo/Rinshan: 手牌は既にツモ牌を含む 14 枚相当 → そのまま渡す
        // Ron/Chankan: 手牌 13 枚 → winTile を追加して 14 枚にする
        var isSelfDraw = request.WinType is WinType.Tsumo or WinType.Rinshan;
        var handKinds = hand.Select(x => x.Kind).ToList();
        if (!isSelfDraw)
        {
            handKinds.Add(request.WinTile.Kind);
        }
        var handTileKindList = new TileKindList(handKinds);
        var scoringCallList = callList.ToScoringCallList();
        var doraIndicators = CollectIndicators(round.Wall, (w, n) => w.GetDoraIndicator(n));
        var isRiichi = round.PlayerRoundStatusArray[winnerIndex].IsRiichi ||
            round.PlayerRoundStatusArray[winnerIndex].IsDoubleRiichi;
        var uradoraIndicators = isRiichi ? CollectIndicators(round.Wall, (w, n) => w.GetUradoraIndicator(n)) : [];
        var winSituation = ScoringConversions.ToWinSituation(request, rules);
        var scoringRules = rules.ToScoringGameRules();
        // 天和は HandValidator 側で winTile=null を要求するため明示的に null を渡す。他の和了種別は通常どおり winTile を渡す
        var winTileKind = winSituation.IsTenhou ? null : request.WinTile.Kind;
        var result = HandCalculator.Calc(
            handTileKindList,
            winTileKind,
            scoringCallList,
            doraIndicators,
            uradoraIndicators,
            winSituation,
            scoringRules
        );

        if (result.ErrorMessage is not null)
        {
            // 役なし等で HandCalculator がエラーを返した場合は例外として扱い、不正な和了応答を SettleWin へ進めない。
            // ResponseCandidateEnumerator は待ち牌一致のみで Ron/TsumoAgari 候補を提示しており役の存在は検証しないため、
            // ここで 0 点和了として黙って通すと未立直役なしロン等が 0 点和了として成立してしまう。
            // 例外は RoundStateContext.ProcessEventAsync が InvalidEventReceived イベントに回送する
            throw new InvalidOperationException(
                $"HandCalculator がエラーを返しました: {result.ErrorMessage} " +
                $"(winnerIndex={winnerIndex.Value}, loserIndex={request.LoserIndex.Value}, " +
                $"winType={request.WinType}, winTile.Id={request.WinTile.Id})"
            );
        }

        var deltas = PointDistribution.Distribute(result, request);
        var isMenzen = callList.All(x => x.Type is CallType.Ankan);
        return new ScoreResult(result.Han, result.Fu, deltas, result.YakuList, isMenzen);
    }

    private static TileKindList CollectIndicators(Wall wall, Func<Wall, int, Tile> getIndicator)
    {
        var builder = new List<TileKind>(wall.DoraRevealedCount);
        for (var n = 0; n < wall.DoraRevealedCount; n++)
        {
            builder.Add(getIndicator(wall, n).Kind);
        }
        return [.. builder];
    }
}
