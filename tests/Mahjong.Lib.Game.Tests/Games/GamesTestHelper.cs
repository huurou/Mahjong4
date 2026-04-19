using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.States.RoundStates;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    /// <summary>
    /// Phase 5 RoundManager 経路用の視点射影の既定実装を返す
    /// </summary>
    internal static IRoundViewProjector CreateProjector()
    {
        return new RoundViewProjector();
    }

    /// <summary>
    /// Phase 5 RoundManager 経路用の合法応答候補列挙の既定実装を返す
    /// </summary>
    internal static IResponseCandidateEnumerator CreateEnumerator(GameRules? rules = null)
    {
        return new ResponseCandidateEnumerator(CreateNoOpTenpaiChecker(), rules ?? new GameRules());
    }

    /// <summary>
    /// Phase 5 RoundManager 経路用の応答優先順位ポリシーの既定実装を返す
    /// </summary>
    internal static IResponsePriorityPolicy CreatePriorityPolicy()
    {
        return new TenhouResponsePriorityPolicy();
    }

    /// <summary>
    /// Phase 5 RoundManager 経路用のタイムアウト時既定応答生成の既定実装を返す
    /// </summary>
    internal static IDefaultResponseFactory CreateDefaultFactory()
    {
        return new DefaultResponseFactory();
    }

    /// <summary>
    /// Phase 5 RoundManager 経路用の no-op トレーサーを返す
    /// </summary>
    internal static IGameTracer CreateTracer()
    {
        return new NullGameTracer();
    }

    /// <summary>
    /// Phase 5 RoundManager 経路用の no-op ロガーファクトリを返す
    /// </summary>
    internal static ILoggerFactory CreateLoggerFactory()
    {
        return NullLoggerFactory.Instance;
    }

    /// <summary>
    /// 既定のサービス群で <see cref="GameManager"/> を生成する
    /// </summary>
    internal static GameManager CreateManager(PlayerList? playerList = null, GameRules? rules = null)
    {
        return new GameManager(
            playerList ?? CreatePlayerList(),
            rules ?? new GameRules(),
            CreateWallGenerator(),
            CreateNoOpScoreCalculator(),
            CreateNoOpTenpaiChecker(),
            CreateProjector(),
            CreateEnumerator(rules),
            CreatePriorityPolicy(),
            CreateDefaultFactory(),
            CreateTracer(),
            CreateLoggerFactory()
        );
    }
}
