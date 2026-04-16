using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.States.RoundStates;
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

    /// <summary>
    /// 点数計算を行わない既定の <see cref="IScoreCalculator"/> を返す
    /// </summary>
    internal static IScoreCalculator CreateNoOpScoreCalculator()
    {
        return RoundStateContextTestHelper.CreateNoOpScoreCalculator();
    }

    /// <summary>
    /// テンパイ判定を行わない既定の <see cref="ITenpaiChecker"/> を返す
    /// </summary>
    internal static ITenpaiChecker CreateNoOpTenpaiChecker()
    {
        return RoundStateContextTestHelper.CreateNoOpTenpaiChecker();
    }
}
