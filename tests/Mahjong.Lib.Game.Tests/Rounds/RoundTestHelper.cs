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
        mock.Setup(x => x.IsKoutsuOnlyInAllInterpretations(It.IsAny<Hand>(), It.IsAny<CallList>(), It.IsAny<int>()))
            .Returns(true);
        return mock.Object;
    }

    internal static IScoreCalculator NoOpScoreCalculator { get; } = CreateNoOpScoreCalculator();

    private static IScoreCalculator CreateNoOpScoreCalculator()
    {
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(new ScoreResult(0, 0, new PointArray(new Point(0)), []));
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

    /// <summary>
    /// Haipai 実行後に親 (PlayerIndex 0) が幺九 13 種を持つ山牌の <see cref="Round"/> を生成する。
    /// 九種九牌候補が成立する統合テスト用。
    /// <see cref="Wall.Draw"/> は末尾 (tiles[135]) から取るため、親の Haipai 対象位置は末尾側になる:
    /// - 1 巡目 (4 枚): 135, 134, 133, 132
    /// - 2 巡目 (4 枚): 119, 118, 117, 116
    /// - 3 巡目 (4 枚): 103, 102, 101, 100
    /// - 4 巡目 (1 枚): 87
    /// </summary>
    internal static Round CreateRoundWithDealerKyuushuHand()
    {
        // Kind 0 (m1), 8 (m9), 9 (p1), 17 (p9), 18 (s1), 26 (s9), 27-33 (字牌7種) = 13 種
        var positions = new Dictionary<int, int>
        {
            [135] = 0,    // m1
            [134] = 32,   // m9
            [133] = 36,   // p1
            [132] = 68,   // p9
            [119] = 72,   // s1
            [118] = 104,  // s9
            [117] = 108,  // 東
            [116] = 112,  // 南
            [103] = 116,  // 西
            [102] = 120,  // 北
            [101] = 124,  // 白
            [100] = 128,  // 發
            [87]  = 132,  // 中
        };
        var placed = positions.Values.ToHashSet();
        var remaining = Enumerable.Range(0, 136).Where(x => !placed.Contains(x)).ToList();
        var tiles = new Tile[136];
        foreach (var (pos, tileId) in positions)
        {
            tiles[pos] = new Tile(tileId);
        }
        var remainIdx = 0;
        for (var i = 0; i < 136; i++)
        {
            if (tiles[i] is null)
            {
                tiles[i] = new Tile(remaining[remainIdx++]);
            }
        }
        return new Round(
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(25000)),
            new Wall(tiles)
        );
    }
}
