using Mahjong.Lib.Game.AutoPlay.Paifu;
using Mahjong.Lib.Game.AutoPlay.Tracing;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// 4 人 AI 自動対局を指定回数実行するランナー
/// </summary>
public sealed class AutoPlayRunner(
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    IPlayerFactory playerFactory,
    GameRules rules,
    AutoPlayOptions options,
    ILoggerFactory loggerFactory
)
{
    private static TimeSpan GameTimeout { get; } = TimeSpan.FromMinutes(5);

    public async Task<StatsReport> RunAsync(int gameCount, StatsTracer statsTracer, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger<AutoPlayRunner>();
        for (var i = 0; i < gameCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            try
            {
                var result = await RunSingleGameAsync(i, gameCount, statsTracer, ct);
                sw.Stop();
                var percent = (i + 1) * 100.0 / gameCount;
                logger.LogInformation("対局 {GameNumber}/{GameCount} ({Percent:00.00}%) 完了 ({ElapsedSeconds:F1}s) {Ranking}",
                    i + 1, gameCount, percent, sw.Elapsed.TotalSeconds, FormatRanking(result.Points, result.PlayerList));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                sw.Stop();
                statsTracer.RecordGameFailed();
                logger.LogError(ex, "対局 {GameNumber} が例外で中断しました。次の対局に進みます。", i);
                var percent = (i + 1) * 100.0 / gameCount;
                logger.LogInformation("対局 {GameNumber}/{GameCount} ({Percent:00.00}%) 失敗 ({ElapsedSeconds:F1}s)",
                    i + 1, gameCount, percent, sw.Elapsed.TotalSeconds);
            }
        }
        return statsTracer.Build();
    }

    private async Task<(PointArray Points, PlayerList PlayerList)> RunSingleGameAsync(int gameNumber, int gameCount, StatsTracer statsTracer, CancellationToken ct)
    {
        var playerList = CreatePlayers();
        var progressTracer = new ProgressTracer(
            gameNumber,
            gameCount,
            playerList,
            loggerFactory.CreateLogger<ProgressTracer>()
        );

        // 山 seed は options.Seed と gameNumber から派生して決定的に 2496 バイトを生成する
        // (天鳳互換 MT19937 + SHA512 用)。同じ options.Seed と gameNumber なら常に同じ山が生成され、
        // 対局の再現性が担保される。
        var derivedSeed = unchecked(options.Seed * 31 + gameNumber);
        var deterministicRng = new Random(derivedSeed);
        var seedBytes = new byte[2496];
        deterministicRng.NextBytes(seedBytes);
        var gameSeed = Convert.ToBase64String(seedBytes);
        var wallGenerator = new WallGeneratorTenhou(gameSeed);

        var tracers = new List<IGameTracer> { statsTracer, progressTracer };
        StreamWriter? paifuWriter = null;
        TenhouJsonPaifuRecorder? paifuRecorder = null;
        if (options.WritePaifu)
        {
            var title = new[]
            {
                $"AutoPlay {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"対局 {gameNumber + 1}/{gameCount}",
            };
            (paifuWriter, paifuRecorder) = TenhouPaifuFileSink.Create(options.OutputDirectory, playerList, rules, title);
            tracers.Add(paifuRecorder);
        }

        try
        {
            var composite = new CompositeGameTracer(
                tracers,
                loggerFactory.CreateLogger<CompositeGameTracer>()
            );
            using var ctx = new GameStateContext(
                playerList,
                rules,
                wallGenerator,
                projector,
                enumerator,
                priorityPolicy,
                defaultFactory,
                composite,
                loggerFactory.CreateLogger<GameStateContext>(),
                loggerFactory.CreateLogger<RoundStateContext>()
            );
            await ctx.StartAsync(ct);
            progressTracer.SetContext(ctx);
            await WaitForGameEndAsync(ctx, GameTimeout, ct);
            return (ctx.Game.PointArray, playerList);
        }
        finally
        {
            paifuRecorder?.Dispose();
            paifuWriter?.Dispose();
        }
    }

    private static string FormatRanking(PointArray points, PlayerList playerList)
    {
        var ranked = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => (PlayerIndex: x, Point: points[new PlayerIndex(x)].Value, Name: playerList[new PlayerIndex(x)].DisplayName))
            .OrderByDescending(x => x.Point)
            .Select((x, rank) => $"{rank + 1}位 P{x.PlayerIndex}({x.Name})={x.Point}");
        return $"[{string.Join(", ", ranked)}]";
    }

    private PlayerList CreatePlayers()
    {
        var players = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => playerFactory.Create(new PlayerIndex(x), PlayerId.NewId()))
            .ToArray();
        return new PlayerList(players);
    }

    private static async Task WaitForGameEndAsync(GameStateContext ctx, TimeSpan timeout, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is GameStateEnd)
            {
                tcs.TrySetResult();
            }
        }

        ctx.GameStateChanged += Handler;
        try
        {
            if (ctx.State is GameStateEnd) { return; }
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(timeout);
            await tcs.Task.WaitAsync(timeoutCts.Token);
        }
        finally
        {
            ctx.GameStateChanged -= Handler;
        }
    }
}
