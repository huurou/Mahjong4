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
            try
            {
                await RunSingleGameAsync(i, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                statsTracer.RecordGameFailed();
                logger.LogError(ex, "対局 {GameNumber} が例外で中断しました。次の対局に進みます。", i);
            }
        }
        return statsTracer.Build();
    }

    private async Task RunSingleGameAsync(int gameNumber, CancellationToken ct)
    {
        var playerList = CreatePlayers();
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
            tracer,
            loggerFactory.CreateLogger<GameStateContext>(),
            loggerFactory.CreateLogger<RoundStateContext>()
        );

        await manager.StartAsync(ct);
        await WaitForGameEndAsync(manager, GameTimeout, ct);
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
