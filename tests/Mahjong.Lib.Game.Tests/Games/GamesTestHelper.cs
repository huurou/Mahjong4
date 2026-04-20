using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
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
        return new ResponseCandidateEnumerator(rules ?? new GameRules());
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
        IWallGenerator? wallGenerator = null
    )
    {
        var effectiveRules = rules ?? new GameRules();
        return new GameManager(
            playerList ?? CreatePlayerList(),
            effectiveRules,
            wallGenerator ?? CreateWallGenerator(),
            CreateProjector(),
            new ResponseCandidateEnumerator(effectiveRules),
            CreatePriorityPolicy(),
            CreateDefaultFactory(),
            CreateTracer(),
            NullLogger<GameStateContext>.Instance,
            NullLogger<RoundStateContext>.Instance
        );
    }

    /// <summary>
    /// 対局レベル終了状態 (<see cref="Game.States.GameStates.Impl.GameStateEnd"/>) への遷移を待機する
    /// </summary>
    internal static async Task WaitForGameEndAsync(GameManager manager, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is Game.States.GameStates.Impl.GameStateEnd)
            {
                tcs.TrySetResult();
            }
        }

        manager.Context.GameStateChanged += Handler;
        try
        {
            if (manager.Context.State is Game.States.GameStates.Impl.GameStateEnd)
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
    /// 既定のサービス群で <see cref="GameStateContext"/> を生成する
    /// </summary>
    internal static GameStateContext CreateContext(
        IWallGenerator? wallGenerator = null,
        PlayerList? playerList = null,
        GameRules? rules = null
    )
    {
        return new GameStateContext(
            wallGenerator ?? CreateWallGenerator(),
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
