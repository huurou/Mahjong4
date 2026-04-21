using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
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

public class AI_v0_3_0_評価値_OnTsumoTests
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
        var player = CreateAI();

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
        var player = CreateAI();

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
        // - m1 切り → 対子を崩すため 2 シャンテン
        var hand = new Hand(
        [
            new Tile(0), new Tile(1),      // m1 m1
            new Tile(8), new Tile(9),      // m3 m3
            new Tile(36), new Tile(37),    // p1 p1
            new Tile(44), new Tile(45),    // p3 p3
            new Tile(72), new Tile(73),    // s1 s1
            new Tile(80), new Tile(81),    // s3 s3
            new Tile(16),                   // m5
            new Tile(52),                   // p5
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
        var player = await CreateAIWithGameStartAsync(hand: hand);
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
        // - 東 切り → shanten=0、有効牌 {東} (2 枚未見)
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
    public async Task 有効牌同点時_役牌の評価値が高く非役牌が切られる()
    {
        // Arrange: 14 枚 (m123, m456, m789, p123, 東, 南)
        // - 東切り → 南単騎テンパイ、有効牌 {南} スコア 3 (手に 1 枚、未見 3)
        // - 南切り → 東単騎テンパイ、有効牌 {東} スコア 3
        // 有効牌同点。ViewerIndex=0 (東家) + RoundWind.East なので東は自風+場風で役牌。
        // 東の評価値 = 3(unseen) × 2(役牌) = 6、南 = 3 × 1 = 3 → 南が切られる
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(12), new Tile(16), new Tile(20),   // m456
            new Tile(24), new Tile(28), new Tile(32),   // m789
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(108),                               // 東
            new Tile(112),                               // 南
        ]);
        var tileTon = new Tile(108);
        var tileNan = new Tile(112);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileTon, false),
            new DahaiOption(tileNan, false),
        ]);
        var player = await CreateAIWithGameStartAsync(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileTon, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 南 (非役牌) が切られる
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileNan.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 赤ドラは評価値が倍になり非赤ドラの同種牌が優先的に切られる()
    {
        // Arrange: 手牌 14 枚 — 赤ドラ5m(Tile 16) と 通常5m(Tile 17)、他は m123 / p123 / s123 / m789 / 東東
        // m5 を 2 枚持つため、m5 (どちらの Tile でも Kind 同じ) の 13 枚手牌は同じ
        // 有効牌スコアは同じ → 評価値で差がつく: 赤5m は ×2、通常5m は ×1 → 通常5m (Tile 17) が切られる
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(16), new Tile(17),                  // m5 (赤) m5 (通常)
            new Tile(24), new Tile(28), new Tile(32),   // m789
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(72), new Tile(76), new Tile(80),   // s123
        ]);
        var tileAkaFiveMan = new Tile(16);
        var tileNormalFiveMan = new Tile(17);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileAkaFiveMan, false),
            new DahaiOption(tileNormalFiveMan, false),
        ]);
        var player = await CreateAIWithGameStartAsync(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileAkaFiveMan, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 通常5m (Tile 17) が切られる
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileNormalFiveMan.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task くっつき範囲が広い中央牌より範囲が狭い端牌が切られる()
    {
        // Arrange: 14 枚 (p123, p456, p789, s123, m5(孤立), m9(孤立))
        // - m5切り → 面子 4 + m9単騎テンパイ、有効牌 {m9}、未見3枚 (m9 手に 1 枚)
        // - m9切り → 面子 4 + m5単騎テンパイ、有効牌 {m5}、未見3枚
        // 有効牌同点。評価値計算:
        // - m5: くっつき {m3,m4,m5,m6,m7}、全 5 牌が未見 4/4/3/4/4 → 項合計 4+4+3+4+4 = 19
        // - m9: くっつき {m7,m8,m9}、未見 4/4/3 → 項合計 4+4+3 = 11
        // どちらも非役牌・非ドラ → 外側倍率 1 → m9 のほうが評価値が小さく切られる
        var hand = new Hand(
        [
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(48), new Tile(52), new Tile(56),   // p456
            new Tile(60), new Tile(64), new Tile(68),   // p789
            new Tile(72), new Tile(76), new Tile(80),   // s123
            new Tile(16),                                // m5
            new Tile(32),                                // m9
        ]);
        var tileM5 = new Tile(16);
        var tileM9 = new Tile(32);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileM5, false),
            new DahaiOption(tileM9, false),
        ]);
        var player = await CreateAIWithGameStartAsync(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileM5, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: m9 切り (評価値 11 < m5 19)
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileM9.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 対象牌自身が表ドラのとき評価値が倍になり他の非役牌が切られる()
    {
        // Arrange: 14 枚 (m123, m456, m789, p123, 北(孤立), 南(孤立))
        // 東家 + 場風 East なので北・南とも非役牌・非対象風。
        // 有効牌同点 (どちらも単騎テンパイで残りの牌 3 枚が未見)。
        // DoraIndicators に 西指示牌を 1 枚入れる (西→北がドラ) → 北が表ドラ
        // - 北: 3 × 2 = 6、南: 3 × 1 = 3 → 評価値最小は南 → 南が切られる
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(12), new Tile(16), new Tile(20),   // m456
            new Tile(24), new Tile(28), new Tile(32),   // m789
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(120),                               // 北
            new Tile(112),                               // 南
        ]);
        var tilePei = new Tile(120);
        var tileNan = new Tile(112);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tilePei, false),
            new DahaiOption(tileNan, false),
        ]);
        var player = await CreateAIWithGameStartAsync(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var view = CreateView(hand) with { DoraIndicators = [new Tile(116)] };  // 西 → 北がドラ
        var notification = new TsumoNotification(view, tilePei, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 南切り (北は表ドラ × 2 で評価値 6、南は 3)
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileNan.Id, dahai.Tile.Id);
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
        var player1 = await CreateAIWithGameStartAsync(seed: 100);
        var player2 = await CreateAIWithGameStartAsync(seed: 100);
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

    [Fact]
    public async Task 非東家視点_自風_役牌倍率が適用され自風が切られない()
    {
        // Arrange: ViewerIndex=1 (南家) + RoundWind=East で自風=南。
        // 14 枚 (m123, m456, m789, p123, 南, 北) で有効牌同点 (どちらの単騎もunseen 3)。
        // 南 は自風で役牌 → outer=2、評価値=6。北は非役牌 → outer=1、評価値=3。
        // 評価値最小の北が切られる (南は残す)
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(12), new Tile(16), new Tile(20),   // m456
            new Tile(24), new Tile(28), new Tile(32),   // m789
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(112),                               // 南
            new Tile(120),                               // 北
        ]);
        var tileNan = new Tile(112);
        var tilePei = new Tile(120);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tileNan, false),
            new DahaiOption(tilePei, false),
        ]);
        var player = await CreateAIAtSeatWithGameStartAsync(new PlayerIndex(1));
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var view = CreateViewForSeat(new PlayerIndex(1), hand);
        var notification = new TsumoNotification(view, tileNan, candidates, [new PlayerIndex(1)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 北切り (非役牌の低評価)
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tilePei.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 複数ドラ指示牌_対象牌の評価値が2乗倍される()
    {
        // Arrange: 14 枚 (m123, m456, m789, p123, 北, 南)、DoraIndicators=[西,西] で北が2重ドラ。
        // 北切り: outer = 2^2 = 4、value = 3 × 4 = 12
        // 南切り: outer = 1、value = 3
        // 評価値最小の南が切られる
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),      // m123
            new Tile(12), new Tile(16), new Tile(20),   // m456
            new Tile(24), new Tile(28), new Tile(32),   // m789
            new Tile(36), new Tile(40), new Tile(44),   // p123
            new Tile(120),                               // 北
            new Tile(112),                               // 南
        ]);
        var tilePei = new Tile(120);
        var tileNan = new Tile(112);
        var options = new DahaiOptionList(
        [
            new DahaiOption(tilePei, false),
            new DahaiOption(tileNan, false),
        ]);
        var player = await CreateAIWithGameStartAsync(hand: hand);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var view = CreateView(hand) with { DoraIndicators = [new Tile(116), new Tile(117)] };  // 西×2 → 北ダブドラ
        var notification = new TsumoNotification(view, tilePei, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 南切り (北評価値 12 > 南 3)
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileNan.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 評価値計算到達時にrules未設定_例外()
    {
        // Arrange: OnGameStart 未受信で評価値計算に到達させる。
        // 14 枚 (m123, m456, m789, p123, 東, 南) で有効牌同点 → Phase 2 に入る。
        // rules_ が null なので CalcEvaluation が InvalidOperationException を投げる
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(16), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(36), new Tile(40), new Tile(44),
            new Tile(108),
            new Tile(112),
        ]);
        var options = new DahaiOptionList(
        [
            new DahaiOption(new Tile(108), false),
            new DahaiOption(new Tile(112), false),
        ]);
        var player = CreateAI();  // OnGameStart 未呼出
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), new Tile(108), candidates, [new PlayerIndex(0)]);

        // Act
        var ex = await Record.ExceptionAsync(async () => await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    private static AI_v0_3_0_評価値 CreateAI(int seed = 42, Hand? hand = null)
    {
        return new AI_v0_3_0_評価値(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
    }

    private static async Task<AI_v0_3_0_評価値> CreateAIWithGameStartAsync(int seed = 42, Hand? hand = null)
    {
        var player = CreateAI(seed, hand);
        var rules = new GameRules();
        var others = Enumerable.Range(1, 3)
            .Select(x => new AI_v0_3_0_評価値(PlayerId.NewId(), new PlayerIndex(x), new Random(seed + x)));
        var playerList = new PlayerList([player, .. others]);
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));
        await player.OnGameStartAsync(notification, TestContext.Current.CancellationToken);
        return player;
    }

    private static async Task<AI_v0_3_0_評価値> CreateAIAtSeatWithGameStartAsync(PlayerIndex seat, int seed = 42)
    {
        var player = new AI_v0_3_0_評価値(PlayerId.NewId(), seat, new Random(seed));
        var rules = new GameRules();
        var players = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => x == seat.Value
                ? player
                : new AI_v0_3_0_評価値(PlayerId.NewId(), new PlayerIndex(x), new Random(seed + x)));
        var playerList = new PlayerList(players);
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));
        await player.OnGameStartAsync(notification, TestContext.Current.CancellationToken);
        return player;
    }

    private static PlayerRoundView CreateViewForSeat(PlayerIndex seat, Hand ownHand)
    {
        var visible = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Where(x => x != seat.Value)
            .Select(x => new VisiblePlayerRoundStatus(new PlayerIndex(x), false, false, true));
        return new PlayerRoundView(
            seat,
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
            [.. visible],
            70
        );
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
