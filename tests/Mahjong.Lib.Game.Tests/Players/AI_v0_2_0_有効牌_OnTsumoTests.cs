using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_2_0_有効牌_OnTsumoTests
{
    [Fact]
    public async Task TsumoAgariCandidateあり_TsumoAgariResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList(
        [
            new TsumoAgariCandidate(),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(0), false)])),
        ]);
        var notification = new TsumoNotification(CreateView(new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)))), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<TsumoAgariResponse>(response);
    }

    [Fact]
    public async Task DahaiCandidateあり_有効なDahaiResponseを返す()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, false)]);
        var player = CreateAI(hand: hand);

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
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
        var player = CreateAI(hand: hand);

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.True(dahai.IsRiichi);
    }

    [Fact]
    public async Task シャンテンが最小になる打牌候補が選ばれる()
    {
        // Arrange: 七対子 6 対 + 孤立 2 枚 (m5, p5) の 14 枚手牌
        // - m5 切り / p5 切り → 七対子テンパイ (shanten=0)
        // - m1 切り → 七対子 2 シャンテン (対子を崩す)
        // AI は m1 切りを選ばないはず
        var hand = new Hand(
        [
            new Tile(0), new Tile(1),      // m1 m1
            new Tile(8), new Tile(9),      // m3 m3
            new Tile(36), new Tile(37),    // p1 p1
            new Tile(44), new Tile(45),    // p3 p3
            new Tile(72), new Tile(73),    // s1 s1
            new Tile(80), new Tile(81),    // s3 s3
            new Tile(16),                   // m5 (孤立)
            new Tile(52),                   // p5 (孤立)
        ]);
        var tileM5 = new Tile(16);
        var tileP5 = new Tile(52);
        var tileM1 = new Tile(0);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileM5, false),
            new DahaiOption(tileP5, false),
            new DahaiOption(tileM1, false),
        ]);
        var player = CreateAI(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileM5, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: m1 切りは 2 シャンテンになるため選ばれない
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.NotEqual(tileM1.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task シャンテン同値で有効牌枚数が最多の打牌候補が選ばれる()
    {
        // Arrange: 14 枚和了形 m123 p123 s123 m456 + 東東
        // - m1 切り → shanten=0、有効牌 {m1, m4} (6 枚未見)
        // - 東 切り → shanten=0、有効牌 {東} (2 枚未見、手に東 2 枚なので)
        // AI は有効牌数が多い m1 切りを選ぶ
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(72), new Tile(76), new Tile(80),   // s123
            new Tile(12), new Tile(16), new Tile(20),   // m456
            new Tile(108), new Tile(109),                // 東東
        ]);
        var tileM1 = new Tile(0);
        var tileTon = new Tile(108);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileM1, false),
            new DahaiOption(tileTon, false),
        ]);
        var player = CreateAI(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileM1, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileM1.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 同一シードで同じ打牌を返す()
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

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var player1 = CreateAI(seed: 100, hand: hand);
        var player2 = CreateAI(seed: 100, hand: hand);
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
        var player = CreateAI();
        var candidates = new CandidateList([]);
        var notification = new TsumoNotification(CreateView(new Hand()), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var ex = await Record.ExceptionAsync(async () => await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    private static AI_v0_2_0_有効牌 CreateAI(int seed = 42, Hand? hand = null)
    {
        return new AI_v0_2_0_有効牌(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
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
