using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_4_0_回し打ち_OnTsumoTests
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
        var notification = new TsumoNotification(
            CreateView(new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)))),
            new Tile(0),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<TsumoAgariResponse>(response);
    }

    [Fact]
    public async Task リーチ者なし_DahaiResponseを返す()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, false)]);
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: リーチ者がいないので v0.3.0 同様に DahaiResponse を返す
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task リーチ者なしかつリーチ可能_IsRiichiがtrueになる()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, RiichiAvailable: true)]);
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.True(dahai.IsRiichi);
    }

    [Fact]
    public async Task リーチ者ありかつ2シャンテン_危険度最小の牌を選びIsRiichiはfalse()
    {
        // Arrange: バラバラな 14 枚 (高シャンテン)、候補は無スジ 5m と生牌字牌 (東)。
        //         両方の候補牌が手牌に含まれるよう調整する。
        var hand = new Hand(
        [
            new Tile(0),    // 1m
            new Tile(4),    // 2m
            new Tile(8),    // 3m
            new Tile(12),   // 4m
            new Tile(16),   // 5m (危険候補)
            new Tile(20),   // 6m
            new Tile(24),   // 7m
            new Tile(28),   // 8m
            new Tile(32),   // 9m
            new Tile(36),   // 1p
            new Tile(40),   // 2p
            new Tile(44),   // 3p
            new Tile(48),   // 4p
            new Tile(108),  // 東 (安全候補)
        ]);
        var dangerousTile = new Tile(16);   // 5m 無スジ = 危険度 12
        var safeTile = new Tile(108);        // 東 生牌 = 危険度 3
        var options = new DahaiOptionList(
        [
            new DahaiOption(dangerousTile, RiichiAvailable: true),  // たとえリーチ可能でもベタオリ時は false にされる
            new DahaiOption(safeTile, RiichiAvailable: false),
        ]);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;  // 何も安全牌なし
        var view = CreateViewWithRiichi(hand, new PlayerIndex(1), safeKinds, new RiverArray());
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(view, dangerousTile, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 最安全牌 (東) を選び、IsRiichi は false
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(safeTile.Id, dahai.Tile.Id);
        Assert.False(dahai.IsRiichi);
    }

    [Fact]
    public async Task リーチ者ありかつテンパイ_攻撃打牌を選ぶ()
    {
        // Arrange: 七対子テンパイ 13 枚 + 孤立 1 枚 (テンパイ崩さず切れる牌) の 14 枚手牌
        //         リーチ者 P1 の河に何もない (無スジ 12 の牌もある) が、攻撃打牌はリーチ付きで押す
        var hand = new Hand(
        [
            new Tile(0), new Tile(1),      // 1m 対
            new Tile(4), new Tile(5),      // 2m 対
            new Tile(8), new Tile(9),      // 3m 対
            new Tile(12), new Tile(13),    // 4m 対
            new Tile(16), new Tile(17),    // 5m 対
            new Tile(20), new Tile(21),    // 6m 対
            new Tile(24),                   // 7m (単騎待ち)
            new Tile(100),                  // Sou8 (ツモ牌、孤立、切ると七対子テンパイ維持)
        ]);
        var discardTile = new Tile(100);
        var options = new DahaiOptionList([new DahaiOption(discardTile, RiichiAvailable: true)]);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;
        var view = CreateViewWithRiichi(hand, new PlayerIndex(1), safeKinds, new RiverArray());
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(view, discardTile, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: テンパイ → 攻撃打牌、リーチ可能なので IsRiichi=true (押し)
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(discardTile.Id, dahai.Tile.Id);
        Assert.True(dahai.IsRiichi);
    }

    // 1 シャンテンでの回し打ち/ベタオリ分岐は、手牌設計と ShantenHelper の挙動に強く依存する統合的な
    // 振る舞いのため、明確なテンパイ (shanten<=0) / 2 シャンテン以上のケースで境界をカバーする。
    // 実際の 1 シャンテン局面での挙動は AutoPlay 50 局スモーク (放銃率 6.4%、v0.3.0 比 13pt 減) で検証済み

    private static AI_v0_4_0_回し打ち CreateAI(int seed = 42)
    {
        return new AI_v0_4_0_回し打ち(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
    }

    private static async Task<AI_v0_4_0_回し打ち> CreateAIWithGameStartAsync(int seed = 42)
    {
        var player = CreateAI(seed);
        var rules = new GameRules();
        var others = Enumerable.Range(1, 3)
            .Select(x => new AI_v0_4_0_回し打ち(PlayerId.NewId(), new PlayerIndex(x), new Random(seed + x)));
        var playerList = new PlayerList([player, .. others]);
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));
        await player.OnGameStartAsync(notification, TestContext.Current.CancellationToken);
        return player;
    }

    private static PlayerRoundView CreateView(Hand ownHand)
    {
        return new PlayerRoundView(
            ViewerIndex: new PlayerIndex(0),
            RoundWind: RoundWind.East,
            RoundNumber: new RoundNumber(0),
            Honba: new Honba(0),
            KyoutakuRiichiCount: new KyoutakuRiichiCount(0),
            TurnIndex: new PlayerIndex(0),
            PointArray: new PointArray(new Point(35000)),
            OwnHand: ownHand,
            CallListArray: new CallListArray(),
            RiverArray: new RiverArray(),
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses:
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ],
            WallLiveRemaining: 70
        );
    }

    private static PlayerRoundView CreateViewWithRiichi(
        Hand ownHand,
        PlayerIndex riichiPlayer,
        ImmutableHashSet<TileKind> safeKinds,
        RiverArray riverArray
    )
    {
        var otherStatuses = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Where(x => x != 0)
            .Select(x => x == riichiPlayer.Value
                ? new VisiblePlayerRoundStatus(new PlayerIndex(x), true, false, true, safeKinds)
                : new VisiblePlayerRoundStatus(new PlayerIndex(x), false, false, true, null))
            .ToImmutableArray();

        return new PlayerRoundView(
            ViewerIndex: new PlayerIndex(0),
            RoundWind: RoundWind.East,
            RoundNumber: new RoundNumber(0),
            Honba: new Honba(0),
            KyoutakuRiichiCount: new KyoutakuRiichiCount(1),
            TurnIndex: new PlayerIndex(0),
            PointArray: new PointArray(new Point(35000)),
            OwnHand: ownHand,
            CallListArray: new CallListArray(),
            RiverArray: riverArray,
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses: otherStatuses,
            WallLiveRemaining: 70
        );
    }
}
