using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_2_0_有効牌_OnTsumoTests
{
    [Fact]
    public async Task TsumoAgariCandidateあり_TsumoAgariResponseを返す()
    {
        // Arrange
        var player = CreateAI(new FakeShantenEvaluator());
        var candidates = new CandidateList(
        [
            new TsumoAgariCandidate(),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(0), false)])),
        ]);
        var notification = new TsumoNotification(CreateView(new Hand()), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<TsumoAgariResponse>(response);
    }

    [Fact]
    public async Task シャンテンが最小になる打牌候補が選ばれる()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var tileB = new Tile(4);
        var tileC = new Tile(8);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileA, false),
            new DahaiOption(tileB, false),
            new DahaiOption(tileC, false),
        ]);

        var evaluator = new FakeShantenEvaluator(
            calcShanten: (h, _) =>
            {
                var ids = h.Select(x => x.Id).ToHashSet();
                if (!ids.Contains(tileA.Id)) { return 0; } // tileA を抜いた
                if (!ids.Contains(tileB.Id)) { return 1; } // tileB を抜いた
                if (!ids.Contains(tileC.Id)) { return 2; } // tileC を抜いた
                return 3;
            }
        );
        var player = CreateAI(evaluator, hand: hand);

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
        Assert.False(dahai.IsRiichi);
    }

    [Fact]
    public async Task シャンテン同値で有効牌枚数が最多の打牌候補が選ばれる()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var tileB = new Tile(4);
        var tileC = new Tile(8);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileA, false),
            new DahaiOption(tileB, false),
            new DahaiOption(tileC, false),
        ]);

        // すべて同じシャンテンだが有効牌が異なる: tileB 抜きが最多 (牌種 20 = 4枚未見)
        var evaluator = new FakeShantenEvaluator(
            calcShanten: (_, _) => 0,
            enumerateUseful: (h, _) =>
            {
                var ids = h.Select(x => x.Id).ToHashSet();
                if (!ids.Contains(tileA.Id)) { return [18]; }     // 1種
                if (!ids.Contains(tileB.Id)) { return [19, 20]; } // 2種 (8枚分)
                if (!ids.Contains(tileC.Id)) { return [21]; }     // 1種
                return [];
            }
        );
        var player = CreateAI(evaluator, hand: hand);

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileB.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 選ばれた候補がリーチ可能_IsRiichiがtrueになる()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileA, RiichiAvailable: true),
        ]);
        var evaluator = new FakeShantenEvaluator(calcShanten: (_, _) => 0);
        var player = CreateAI(evaluator, hand: hand);

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.True(dahai.IsRiichi);
    }

    [Fact]
    public async Task 全候補同点_同一シードで同じ打牌を返す()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var options = new DahaiOptionList(
        [
            new DahaiOption(new Tile(0), false),
            new DahaiOption(new Tile(4), false),
            new DahaiOption(new Tile(8), false),
            new DahaiOption(new Tile(12), false),
        ]);

        var evaluator = new FakeShantenEvaluator(calcShanten: (_, _) => 0);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act: 同一シードで 2 個の player を作り、同じ入力に対して同じ応答が返ることを確認
        var player1 = CreateAI(evaluator, seed: 100, hand: hand);
        var player2 = CreateAI(evaluator, seed: 100, hand: hand);
        var responses1 = new List<int>();
        var responses2 = new List<int>();
        for (var i = 0; i < 10; i++)
        {
            var r1 = (DahaiResponse)await player1.OnTsumoAsync(notification, TestContext.Current.CancellationToken);
            var r2 = (DahaiResponse)await player2.OnTsumoAsync(notification, TestContext.Current.CancellationToken);
            responses1.Add(r1.Tile.Id);
            responses2.Add(r2.Tile.Id);
        }

        // Assert
        Assert.Equal(responses1, responses2);
    }

    [Fact]
    public async Task DahaiCandidateなし_例外()
    {
        // Arrange
        var player = CreateAI(new FakeShantenEvaluator());
        var candidates = new CandidateList([]);
        var notification = new TsumoNotification(CreateView(new Hand()), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var ex = await Record.ExceptionAsync(async () => await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    private static AI_v0_2_0_有効牌 CreateAI(IShantenEvaluator evaluator, int seed = 42, Hand? hand = null)
    {
        return new AI_v0_2_0_有効牌(PlayerId.NewId(), new PlayerIndex(0), new Random(seed), evaluator);
    }

    private static PlayerRoundView CreateView(Hand ownHand)
    {
        return new PlayerRoundView(
            new PlayerIndex(0),
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(35000)),
            ownHand,
            new CallListArray(),
            new RiverArray(),
            [],
            new OwnRoundStatus(false, false, false, true, false, false, false),
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true),
            ],
            70
        );
    }
}
