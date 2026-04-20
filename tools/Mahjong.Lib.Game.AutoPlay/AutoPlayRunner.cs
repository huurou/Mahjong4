using Mahjong.Lib.Game.AutoPlay.Tracing;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// 4 人 AI 自動対局を指定回数実行するランナー
/// </summary>
public sealed class AutoPlayRunner(
    IWallGenerator wallGenerator,
    IScoreCalculator scoreCalculator,
    ITenpaiChecker tenpaiChecker,
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    IGameTracer tracer,
    IPlayerFactory playerFactory,
    GameRules rules,
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
                var result = await RunSingleGameAsync(i, gameCount, ct);
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

    private async Task<(PointArray Points, PlayerList PlayerList)> RunSingleGameAsync(int gameNumber, int gameCount, CancellationToken ct)
    {
        var playerList = CreatePlayers();
        var progressTracer = new ProgressTracer(
            gameNumber,
            gameCount,
            playerList,
            loggerFactory.CreateLogger<ProgressTracer>()
        );
        var composite = new CompositeGameTracer(
            [tracer, progressTracer],
            loggerFactory.CreateLogger<CompositeGameTracer>()
        );
        using var manager = new GameManager(
            playerList,
            rules,
            wallGenerator,
            scoreCalculator,
            tenpaiChecker,
            projector,
            enumerator,
            priorityPolicy,
            defaultFactory,
            composite,
            loggerFactory.CreateLogger<GameStateContext>(),
            loggerFactory.CreateLogger<RoundStateContext>()
        );

        await manager.StartAsync(ct);
        progressTracer.SetContext(manager.Context);
        await WaitForGameEndAsync(manager, GameTimeout, ct);
        return (manager.Context.Game.PointArray, playerList);
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

    private static async Task WaitForGameEndAsync(GameManager manager, TimeSpan timeout, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is GameStateEnd)
            {
                tcs.TrySetResult();
            }
        }

        manager.Context.GameStateChanged += Handler;
        try
        {
            if (manager.Context.State is GameStateEnd) { return; }
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(timeout);
            await tcs.Task.WaitAsync(timeoutCts.Token);
        }
        finally
        {
            manager.Context.GameStateChanged -= Handler;
        }
    }
}
