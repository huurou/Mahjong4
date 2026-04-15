using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;

namespace Mahjong.Lib.Game.Tests.Rounds;

internal static class RoundTestHelper
{
    internal static Round CreateRound(int roundNumber = 0)
    {
        var wallGeneratorMock = new Mock<IWallGenerator>();
        wallGeneratorMock
            .Setup(x => x.Generate())
            .Returns(new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x))));
        return new Round(
            RoundWind.East,
            new RoundNumber(roundNumber),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(roundNumber),
            new PointArray(new Point(25000)),
            wallGeneratorMock.Object
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
