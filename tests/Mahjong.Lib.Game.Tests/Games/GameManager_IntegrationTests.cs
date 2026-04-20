using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.States.GameStates;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameManager_IntegrationTests
{
    private static TimeSpan TestTimeout { get; } = TimeSpan.FromSeconds(30);

    private static FakePlayer[] CreatePlayers()
    {
        return [.. Enumerable.Range(0, 4).Select(FakePlayer.Create)];
    }

    /// <summary>
    /// n 回目の <see cref="RoundStartNotification"/> 受信で完了する TaskCompletionSource を返す
    /// </summary>
    private static (TaskCompletionSource<RoundStartNotification> Tcs, Func<RoundStartNotification, CancellationToken, OkResponse> Handler) CreateRoundStartWaiter(int targetCount)
    {
        var tcs = new TaskCompletionSource<RoundStartNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;
        OkResponse Handler(RoundStartNotification notification, CancellationToken ct)
        {
            var current = Interlocked.Increment(ref count);
            if (current == targetCount)
            {
                tcs.TrySetResult(notification);
            }
            return new OkResponse();
        }
        return (tcs, Handler);
    }

    /// <summary>
    /// 親 (index 0) が 1 巡目ツモで タンヤオ 14 枚和了するように構築した 136 枚の山
    /// 親の配牌 + 第1ツモ位置に m234 m567 p234 s234 p5p5 の 14 IDs を配置する
    /// </summary>
    private static int[] BuildDealerTsumoAgariWall()
    {
        var positions = new Dictionary<int, int>
        {
            [135] = 4, [134] = 8, [133] = 12, [132] = 16,   // 1 巡目配牌: m2 m3 m4 m5
            [119] = 20, [118] = 24, [117] = 40, [116] = 44,  // 2 巡目配牌: m6 m7 p2 p3
            [103] = 48, [102] = 76, [101] = 80, [100] = 84,  // 3 巡目配牌: p4 s2 s3 s4
            [87] = 53,                                         // 4 巡目配牌: p5
            [83] = 52,                                         // 親第1ツモ: p5 (winTile)
        };
        return BuildWallFromPositions(positions);
    }

    /// <summary>
    /// 親 (index 0) が 1 巡目ツモで s3 を引いてそのまま打牌し、
    /// 子 1 (index 1) が m234 m567 p234 p55 s24 (s3 カンチャン待ちタンヤオ聴牌) でロン和了する 136 枚の山
    /// </summary>
    private static int[] BuildOyaDahaiKoRonWall()
    {
        var positions = new Dictionary<int, int>
        {
            // 親 (index 0) 配牌 13 枚: 役なし、s3 (kind 20) を含まない
            [135] = 0, [134] = 1, [133] = 28, [132] = 29,    // m1 m1 m8 m8
            [119] = 56, [118] = 57, [117] = 60, [116] = 61,  // p6 p6 p7 p7
            [103] = 88, [102] = 89, [101] = 108, [100] = 109, // s6 s6 東 東
            [87] = 124,                                        // 白
            // 親 第1ツモ位置: s3 (id 80)
            [83] = 80,
            // 子 1 (index 1) 配牌 13 枚: m234 m567 p234 p55 s24 (s3 カンチャン待ちタンヤオ聴牌)
            [131] = 4, [130] = 8, [129] = 12, [128] = 16,    // m2 m3 m4 m5
            [115] = 20, [114] = 24, [113] = 40, [112] = 44,  // m6 m7 p2 p3
            [99] = 48, [98] = 52, [97] = 53, [96] = 76,      // p4 p5 p5 s2
            [86] = 84,                                         // s4
        };
        return BuildWallFromPositions(positions);
    }

    private static int[] BuildWallFromPositions(Dictionary<int, int> positions)
    {
        var placed = positions.Values.ToHashSet();
        var remaining = Enumerable.Range(0, 136).Where(x => !placed.Contains(x)).ToList();
        var tileIds = new int[136];
        foreach (var (pos, tileId) in positions)
        {
            tileIds[pos] = tileId;
        }
        var remainIdx = 0;
        for (var i = 0; i < 136; i++)
        {
            if (!positions.ContainsKey(i))
            {
                tileIds[i] = remaining[remainIdx++];
            }
        }
        return tileIds;
    }

    [Fact]
    public async Task 親の打牌で子がロン和了すると対局が終了する()
    {
        // Arrange: 親 (index 0) が 1 巡目ツモで s3 を引きそのまま打牌、
        // 子 1 (index 1) が m234 m567 p234 p55 s24 (s3 カンチャン待ちタンヤオ聴牌) でロン → SingleRound 終了
        var tileIds = BuildOyaDahaiKoRonWall();
        var players = CreatePlayers();
        players[0] = new FakePlayer(players[0].PlayerId, players[0].DisplayName, players[0].PlayerIndex)
        {
            OnTsumo = (n, _) => new DahaiResponse(n.TsumoTile),
        };
        players[1] = new FakePlayer(players[1].PlayerId, players[1].DisplayName, players[1].PlayerIndex)
        {
            OnDahai = (_, _) => new RonResponse(),
        };
        var rules = new GameRules
        {
            Format = GameFormat.SingleRound,
            RenchanCondition = RenchanCondition.None,
        };
        using var manager = GamesTestHelper.CreateManager(
            new PlayerList(players),
            rules,
            wallGenerator: GamesTestHelper.CreateWallGenerator([.. tileIds]));

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);
        await GamesTestHelper.WaitForGameEndAsync(manager, TestTimeout);

        // Assert
        Assert.Contains(players[1].ReceivedNotifications, x => x is WinNotification);
        Assert.Contains(players[0].ReceivedNotifications, x => x is GameEndNotification);
    }

    [Fact]
    public async Task 親ツモ和了で本場が1増加し同一親で次局が始まる()
    {
        // Arrange: 親 1 巡目に TsumoAgari → Renchan → Honba+1。次局 (東一局 1 本場) の開始通知で assertion
        var tileIds = BuildDealerTsumoAgariWall();
        var (tcs, roundStartHandler) = CreateRoundStartWaiter(targetCount: 2);
        var players = CreatePlayers();
        players[0] = new FakePlayer(players[0].PlayerId, players[0].DisplayName, players[0].PlayerIndex)
        {
            OnTsumo = (_, _) => new TsumoAgariResponse(),
            OnRoundStart = roundStartHandler,
        };
        var rules = new GameRules
        {
            Format = GameFormat.Tonpuu,
            RenchanCondition = RenchanCondition.AgariOrTenpai,
        };
        using var manager = GamesTestHelper.CreateManager(
            new PlayerList(players),
            rules,
            wallGenerator: GamesTestHelper.CreateWallGenerator([.. tileIds]));

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);
        var nextRoundStart = await tcs.Task.WaitAsync(TestTimeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, nextRoundStart.RoundNumber.Value); // 親が流れず東一局継続
        Assert.Equal(1, nextRoundStart.Honba.Value);
        Assert.Contains(players[0].ReceivedNotifications, x => x is WinNotification);
    }

    [Fact]
    public async Task RenchanConditionがNoneの場合親ツモ和了でも親が流れる()
    {
        // Arrange: Tonpuu + None ルール、親ツモ和了 1 回で次局 (東二局) へ移行することを検証
        var tileIds = BuildDealerTsumoAgariWall();
        var (tcs, roundStartHandler) = CreateRoundStartWaiter(targetCount: 2);
        var players = CreatePlayers();
        players[0] = new FakePlayer(players[0].PlayerId, players[0].DisplayName, players[0].PlayerIndex)
        {
            OnTsumo = (_, _) => new TsumoAgariResponse(),
            OnRoundStart = roundStartHandler,
        };
        var rules = new GameRules
        {
            Format = GameFormat.Tonpuu,
            RenchanCondition = RenchanCondition.None,
        };
        using var manager = GamesTestHelper.CreateManager(
            new PlayerList(players),
            rules,
            wallGenerator: GamesTestHelper.CreateWallGenerator([.. tileIds]));

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);
        var nextRoundStart = await tcs.Task.WaitAsync(TestTimeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, nextRoundStart.RoundNumber.Value);
        Assert.Equal(0, nextRoundStart.Honba.Value);
    }

    [Fact]
    public async Task 東風戦で全員ノーテン4局連続流局すると対局が終了する()
    {
        // Arrange
        // NoOpTenpaiChecker で全員ノーテン、既定応答で打牌し続けて 4 局連続荒牌平局 → Tonpuu 終了
        var players = CreatePlayers();
        var rules = new GameRules
        {
            Format = GameFormat.Tonpuu,
            RenchanCondition = RenchanCondition.None,
        };
        using var manager = GamesTestHelper.CreateManager(new PlayerList(players), rules);

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);
        await GamesTestHelper.WaitForGameEndAsync(manager, TestTimeout);

        // Assert
        var ryuukyokuCount = players[0].ReceivedNotifications.OfType<RyuukyokuNotification>().Count();
        Assert.Equal(4, ryuukyokuCount);
        Assert.Contains(players[0].ReceivedNotifications, x => x is GameEndNotification);
    }

    [Fact]
    public async Task 九種九牌による途中流局で同一親で本場が1加算される()
    {
        // Arrange
        // CreateRoundWithDealerKyuushuHand の配牌相当の山を直接 IWallGenerator から返す。
        // 親 OnTsumo で KyuushuKyuuhaiResponse → 途中流局 → 同一親で Honba+1、次局開始
        var positions = new Dictionary<int, int>
        {
            [135] = 0, [134] = 32, [133] = 36, [132] = 68,
            [119] = 72, [118] = 104, [117] = 108, [116] = 112,
            [103] = 116, [102] = 120, [101] = 124, [100] = 128,
            [87] = 132,
        };
        var placed = positions.Values.ToHashSet();
        var remaining = Enumerable.Range(0, 136).Where(x => !placed.Contains(x)).ToList();
        var tileIds = new int[136];
        foreach (var (pos, tileId) in positions)
        {
            tileIds[pos] = tileId;
        }
        var remainIdx = 0;
        for (var i = 0; i < 136; i++)
        {
            if (!positions.ContainsKey(i))
            {
                tileIds[i] = remaining[remainIdx++];
            }
        }
        var (tcs, roundStartHandler) = CreateRoundStartWaiter(targetCount: 2);
        var players = CreatePlayers();
        players[0] = new FakePlayer(players[0].PlayerId, players[0].DisplayName, players[0].PlayerIndex)
        {
            OnTsumo = (_, _) => new KyuushuKyuuhaiResponse(),
            OnRoundStart = roundStartHandler,
        };
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        using var manager = GamesTestHelper.CreateManager(
            new PlayerList(players),
            rules,
            wallGenerator: GamesTestHelper.CreateWallGenerator([.. tileIds]));

        // Act
        await manager.StartAsync(TestContext.Current.CancellationToken);
        var nextRoundStart = await tcs.Task.WaitAsync(TestTimeout, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, nextRoundStart.RoundNumber.Value); // 親続行 (同一親で本場+1)
        Assert.Equal(1, nextRoundStart.Honba.Value);
        var ryuukyoku = players[0].ReceivedNotifications.OfType<RyuukyokuNotification>().FirstOrDefault();
        Assert.NotNull(ryuukyoku);
    }
}
