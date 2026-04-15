using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;

namespace Mahjong.Lib.Game.Tests.Games;

internal static class GamesTestHelper
{
    internal static PlayerList CreatePlayerList()
    {
        return new PlayerList(PlayersTestHelper.CreateTestPlayers(4));
    }

    internal static IWallGenerator CreateWallGenerator()
    {
        var mock = new Mock<IWallGenerator>();
        mock.Setup(x => x.Generate())
            .Returns(() => new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x))));
        return mock.Object;
    }
}
