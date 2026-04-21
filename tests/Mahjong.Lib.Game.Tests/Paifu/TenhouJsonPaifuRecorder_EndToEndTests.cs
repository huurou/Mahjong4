using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text.Json;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouJsonPaifuRecorder_EndToEndTests
{
    [Fact]
    public async Task 対局を実走して生成した各行が天鳳JSONスキーマを満たす()
    {
        // Arrange
        var rules = new GameRules();
        var factory = new AI_v0_1_0_ランダムFactory(42);
        var players = new PlayerList([
            factory.Create(new PlayerIndex(0), PlayerId.NewId()),
            factory.Create(new PlayerIndex(1), PlayerId.NewId()),
            factory.Create(new PlayerIndex(2), PlayerId.NewId()),
            factory.Create(new PlayerIndex(3), PlayerId.NewId()),
        ]);
        var seedBytes = new byte[2496];
        RandomNumberGenerator.Fill(seedBytes);
        var wall = new WallGeneratorTenhou(Convert.ToBase64String(seedBytes));

        var sw = new StringWriter();
        using var recorder = new TenhouJsonPaifuRecorder(sw, players, rules, ["E2E Test", ""]);

        using var context = new GameStateContext(
            players,
            rules,
            wall,
            new RoundViewProjector(),
            new ResponseCandidateEnumerator(rules),
            new TenhouResponsePriorityPolicy(),
            new DefaultResponseFactory(),
            recorder,
            NullLogger<GameStateContext>.Instance,
            NullLogger<RoundStateContext>.Instance
        );

        await context.StartAsync(TestContext.Current.CancellationToken);
        await WaitForEndAsync(context, TestContext.Current.CancellationToken);

        // Act
        var lines = sw.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        Assert.NotEmpty(lines);
        foreach (var line in lines)
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("title", out var title));
            Assert.Equal(2, title.GetArrayLength());
            Assert.True(root.TryGetProperty("name", out var name));
            Assert.Equal(4, name.GetArrayLength());
            Assert.True(root.TryGetProperty("rule", out var rule));
            Assert.True(rule.TryGetProperty("disp", out _));
            Assert.True(rule.TryGetProperty("aka", out _));
            Assert.True(root.TryGetProperty("log", out var log));
            Assert.Equal(1, log.GetArrayLength());
            var roundData = log[0];
            Assert.Equal(17, roundData.GetArrayLength());
            // [0] 局数/本場/供託
            var header = roundData[0];
            Assert.Equal(3, header.GetArrayLength());
            // [1] 開始点
            Assert.Equal(4, roundData[1].GetArrayLength());
            // [16] 結果配列は先頭に文字列
            var result = roundData[16];
            Assert.True(result.GetArrayLength() >= 1);
            Assert.Equal(JsonValueKind.String, result[0].ValueKind);
        }
    }

    private static async Task WaitForEndAsync(GameStateContext context, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object? sender, GameStateChangedEventArgs e)
        {
            if (e.State is GameStateEnd)
            {
                tcs.TrySetResult();
            }
        }
        context.GameStateChanged += Handler;
        try
        {
            if (context.State is GameStateEnd) { return; }
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromMinutes(5));
            await tcs.Task.WaitAsync(cts.Token);
        }
        finally
        {
            context.GameStateChanged -= Handler;
        }
    }
}
