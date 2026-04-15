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
}
