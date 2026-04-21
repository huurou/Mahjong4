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
/// 4 人 AI 自動対局を指定回数実行するランナー。
/// 並列対局時は <see cref="Environment.ProcessorCount"/> 個の worker に対局を均等分配し、
/// 各 worker が独立 <see cref="StatsTracer"/> で集計した結果を最後に <see cref="StatsTracer.Merge"/> で統合する
/// </summary>
public sealed class AutoPlayRunner(
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    MixedPlayerFactory playerListFactory,
    GameRules rules,
    AutoPlayOptions options,
    ILoggerFactory loggerFactory
)
{
    private static TimeSpan GameTimeout { get; } = TimeSpan.FromMinutes(5);

    public async Task<StatsReport> RunAsync(int totalGameCount, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger<AutoPlayRunner>();
        var requested = options.Parallelism > 0 ? options.Parallelism : Environment.ProcessorCount;
        var workerCount = Math.Max(1, Math.Min(requested, totalGameCount));
        var gamesPerWorker = (totalGameCount + workerCount - 1) / workerCount;
        var actualTotal = gamesPerWorker * workerCount;

        logger.LogInformation(
            "並列実行: Workers={WorkerCount} 対局/worker={GamesPerWorker} 実対局数={ActualTotal} (指定={Specified})",
            workerCount, gamesPerWorker, actualTotal, totalGameCount
        );

        var tracers = new StatsTracer[workerCount];
        for (var i = 0; i < workerCount; i++)
        {
            tracers[i] = new StatsTracer();
        }

        var completedGameCount = 0;
        var parallelOpts = new ParallelOptions { MaxDegreeOfParallelism = workerCount, CancellationToken = ct };
        await Parallel.ForEachAsync(Enumerable.Range(0, workerCount), parallelOpts, async (workerId, workerCt) =>
        {
            var tracer = tracers[workerId];
            for (var local = 0; local < gamesPerWorker; local++)
            {
                workerCt.ThrowIfCancellationRequested();
                var gameNumber = workerId * gamesPerWorker + local;
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await RunSingleGameAsync(gameNumber, actualTotal, tracer, workerCt);
                    sw.Stop();
                    var done = Interlocked.Increment(ref completedGameCount);
                    var percent = done * 100.0 / actualTotal;
                    logger.LogInformation(
                        "W{WorkerId} 対局 {GameNumber}/{Total} ({Percent:00.00}%) 完了 ({ElapsedSeconds:F1}s)",
                        workerId, done, actualTotal, percent, sw.Elapsed.TotalSeconds
                    );
                }
                catch (OperationCanceledException) when (workerCt.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    tracer.RecordGameFailed();
                    var done = Interlocked.Increment(ref completedGameCount);
                    logger.LogError(ex, "W{WorkerId} 対局 {GameNumber} が例外で中断。", workerId, gameNumber);
                    logger.LogInformation(
                        "W{WorkerId} 対局 {GameNumber}/{Total} ({Percent:00.00}%) 失敗 ({ElapsedSeconds:F1}s)",
                        workerId, done, actualTotal, done * 100.0 / actualTotal, sw.Elapsed.TotalSeconds
                    );
                }
            }
        });

        return StatsTracer.Merge(tracers).Build();
    }

    private async Task<(PointArray Points, PlayerList PlayerList)> RunSingleGameAsync(
        int gameNumber,
        int gameCount,
        StatsTracer statsTracer,
        CancellationToken ct)
    {
        var playerList = playerListFactory.CreatePlayerList(gameNumber);
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
