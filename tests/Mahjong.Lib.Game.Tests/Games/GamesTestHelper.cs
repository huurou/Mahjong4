using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.States.RoundStates;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Immutable;

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
    /// 指定の Tile.Id シーケンスを山として返す <see cref="IWallGenerator"/> を返す。
    /// deterministic な配牌・ツモ順を要求する統合テスト用
    /// </summary>
    internal static IWallGenerator CreateWallGenerator(ImmutableArray<int> tileIds)
    {
        if (tileIds.Length != 136)
        {
            throw new ArgumentException($"山は136枚必要です。実際:{tileIds.Length}枚", nameof(tileIds));
        }
        var mock = new Mock<IWallGenerator>();
        mock.Setup(x => x.Generate())
            .Returns(() => new Wall(tileIds.Select(x => new Tile(x))));
        return mock.Object;
    }

    /// <summary>
    /// 全ての手牌・副露に対して指定の <paramref name="waitKinds"/> を待ち牌種集合として返す <see cref="ITenpaiChecker"/>。
    /// 統合テストで特定の和了シナリオを deterministic に成立させるために使用する
    /// </summary>
    internal static ITenpaiChecker CreatePermissiveTenpaiChecker(IEnumerable<int> waitKinds)
    {
        var waits = waitKinds.ToImmutableHashSet();
        var mock = new Mock<ITenpaiChecker>();
        mock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(waits.Count > 0);
        mock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns(waits);
        mock.Setup(x => x.IsKoutsuOnlyInAllInterpretations(It.IsAny<Hand>(), It.IsAny<CallList>(), It.IsAny<int>()))
            .Returns(true);
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
    /// 視点射影の既定実装を返す
    /// </summary>
    internal static IRoundViewProjector CreateProjector()
    {
        return new RoundViewProjector();
    }

    /// <summary>
    /// 合法応答候補列挙の既定実装を返す
    /// </summary>
    internal static IResponseCandidateEnumerator CreateEnumerator(GameRules? rules = null)
    {
        return new ResponseCandidateEnumerator(CreateNoOpTenpaiChecker(), rules ?? new GameRules());
    }

    /// <summary>
    /// 応答優先順位ポリシーの既定実装を返す
    /// </summary>
    internal static IResponsePriorityPolicy CreatePriorityPolicy()
    {
        return new TenhouResponsePriorityPolicy();
    }

    /// <summary>
    /// タイムアウト時既定応答生成の既定実装を返す
    /// </summary>
    internal static IDefaultResponseFactory CreateDefaultFactory()
    {
        return new DefaultResponseFactory();
    }

    /// <summary>
    /// no-op トレーサーを返す
    /// </summary>
    internal static IGameTracer CreateTracer()
    {
        return NullGameTracer.Instance;
    }

    /// <summary>
    /// 既定のサービス群で <see cref="GameManager"/> を生成する
    /// </summary>
    internal static GameManager CreateManager(
        PlayerList? playerList = null,
        GameRules? rules = null,
        IWallGenerator? wallGenerator = null,
        ITenpaiChecker? tenpaiChecker = null
    )
    {
        var checker = tenpaiChecker ?? CreateNoOpTenpaiChecker();
        var effectiveRules = rules ?? new GameRules();
        return new GameManager(
            playerList ?? CreatePlayerList(),
            effectiveRules,
            wallGenerator ?? CreateWallGenerator(),
            CreateNoOpScoreCalculator(),
            checker,
            CreateProjector(),
            new ResponseCandidateEnumerator(checker, effectiveRules),
            CreatePriorityPolicy(),
            CreateDefaultFactory(),
            CreateTracer(),
            NullLogger<GameStateContext>.Instance,
            NullLogger<RoundStateContext>.Instance
        );
    }

    /// <summary>
    /// 対局レベル終了状態 (<see cref="Mahjong.Lib.Game.States.GameStates.Impl.GameStateEnd"/>) への遷移を待機する
    /// </summary>
    internal static async Task WaitForGameEndAsync(GameManager manager, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is Mahjong.Lib.Game.States.GameStates.Impl.GameStateEnd)
            {
                tcs.TrySetResult();
            }
        }

        manager.Context.GameStateChanged += Handler;
        try
        {
            if (manager.Context.State is Mahjong.Lib.Game.States.GameStates.Impl.GameStateEnd)
            {
                return;
            }
            await tcs.Task.WaitAsync(timeout.Value);
        }
        finally
        {
            manager.Context.GameStateChanged -= Handler;
        }
    }

    /// <summary>
    /// 既定のサービス群で <see cref="Mahjong.Lib.Game.States.GameStates.GameStateContext"/> を生成する
    /// </summary>
    internal static GameStateContext CreateContext(
        IWallGenerator? wallGenerator = null,
        IScoreCalculator? scoreCalculator = null,
        ITenpaiChecker? tenpaiChecker = null,
        PlayerList? playerList = null,
        GameRules? rules = null
    )
    {
        return new GameStateContext(
            wallGenerator ?? CreateWallGenerator(),
            scoreCalculator ?? CreateNoOpScoreCalculator(),
            tenpaiChecker ?? CreateNoOpTenpaiChecker(),
            playerList ?? CreatePlayerList(),
            CreateProjector(),
            CreateEnumerator(rules),
            CreatePriorityPolicy(),
            CreateDefaultFactory(),
            CreateTracer(),
            NullLogger<GameStateContext>.Instance,
            NullLogger<RoundStateContext>.Instance
        );
    }
}
