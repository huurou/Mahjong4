using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;

namespace Mahjong.Lib.Game.Tests.Rounds;

internal static class RoundTestHelper
{
    /// <summary>
    /// テンパイ判定を行わない既定の <see cref="ITenpaiChecker"/> (常に false / 空集合)
    /// </summary>
    internal static ITenpaiChecker NoOpTenpaiChecker { get; } = CreateNoOpTenpaiChecker();

    private static ITenpaiChecker CreateNoOpTenpaiChecker()
    {
        var mock = new Mock<ITenpaiChecker>();
        mock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(false);
        mock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([]);
        return mock.Object;
    }

    internal static IScoreCalculator NoOpScoreCalculator { get; } = CreateNoOpScoreCalculator();

    private static IScoreCalculator CreateNoOpScoreCalculator()
    {
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(new ScoreResult(0, 0, new PointArray(new Point(0))));
        return mock.Object;
    }

    internal static Round CreateRound(int roundNumber = 0)
    {
        return new Round(
            RoundWind.East,
            new RoundNumber(roundNumber),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(roundNumber),
            new PointArray(new Point(25000)),
            new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)))
        );
    }

    /// <summary>
    /// 指定プレイヤーの手牌だけを差し替えた新しい <see cref="Round"/> を返す
    /// </summary>
    internal static Round InjectHand(Round round, PlayerIndex index, IEnumerable<Tile> tiles)
    {
        var handArray = round.HandArray;
        foreach (var t in handArray[index].ToList())
        {
            handArray = handArray.RemoveTile(index, t);
        }
        handArray = handArray.AddTiles(index, tiles);
        return round with { HandArray = handArray };
    }

    /// <summary>
    /// 4人分の手牌をまとめて差し替えた新しい <see cref="Round"/> を返す
    /// </summary>
    internal static Round InjectHands(Round round, IReadOnlyList<IEnumerable<Tile>> hands)
    {
        if (hands.Count != 4)
        {
            throw new ArgumentException($"手牌は4人分必要です。実際:{hands.Count}人分", nameof(hands));
        }
        for (var i = 0; i < 4; i++)
        {
            round = InjectHand(round, new PlayerIndex(i), hands[i]);
        }
        return round;
    }
}
