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

public class AI_v0_5_0_鳴き_OnDahaiTests
{
    [Fact]
    public async Task RonCandidateあり_RonResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new RonCandidate()]);
        var notification = new DahaiNotification(
            CreateView(new Hand(Enumerable.Range(0, 13).Select(x => new Tile(x * 4)))),
            new Tile(0),
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RonResponse>(response);
    }

    [Fact]
    public async Task 候補が存在しない_OkResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([]);
        var notification = new DahaiNotification(
            CreateView(new Hand(Enumerable.Range(0, 13).Select(x => new Tile(x * 4)))),
            new Tile(0),
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 候補が空 → OkResponse (スルー)
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 自分テンパイ時_PonCandidateあり_OkResponseを返す()
    {
        // Arrange: テンパイ手牌 (3 面子 + 雀頭 + 浮き対子 1) で Haku 対子あり。テンパイなら副露しない。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku, TileKind.Haku
        );
        var discardedHaku = new Tile(TileKind.Haku.Value * 4 + 2);
        var candidates = new CandidateList(
        [
            new PonCandidate([new Tile(TileKind.Haku.Value * 4), new Tile(TileKind.Haku.Value * 4 + 1)]),
        ]);
        var notification = new DahaiNotification(
            CreateView(hand),
            discardedHaku,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 他家リーチ中かつ高シャンテン_PonCandidateあり_OkResponseを返す()
    {
        // Arrange: 孤立主体で高シャンテン、Ton 対子のみペア。他家 P1 がリーチ中ならベタオリしてポンしない。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man5, TileKind.Man9,
            TileKind.Pin1, TileKind.Pin4, TileKind.Pin7,
            TileKind.Sou2, TileKind.Sou5, TileKind.Sou8,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku, TileKind.Chun
        );
        var discardedTon = new Tile(TileKind.Ton.Value * 4 + 2);
        var candidates = new CandidateList(
        [
            new PonCandidate([new Tile(TileKind.Ton.Value * 4), new Tile(TileKind.Ton.Value * 4 + 1)]),
        ]);
        var notification = new DahaiNotification(
            CreateViewWithRiichiOpponent(hand, riichiOpponentIndex: 1),
            discardedTon,
            new PlayerIndex(1),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 役ありシャンテンが進むポン_PonResponseを返す()
    {
        // Arrange: 3 面子 + Haku 対子 + 孤立 2 枚。Haku ポンで翻牌経路が門前シャンテン 1 → テンパイに進む。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Haku, TileKind.Haku,
            TileKind.Ton, TileKind.Nan
        );
        var discardedHaku = new Tile(TileKind.Haku.Value * 4 + 2);
        var ponHandTiles = ImmutableArray.Create(
            new Tile(TileKind.Haku.Value * 4),
            new Tile(TileKind.Haku.Value * 4 + 1));
        var candidates = new CandidateList(
        [
            new PonCandidate(ponHandTiles),
        ]);
        var notification = new DahaiNotification(
            CreateView(hand),
            discardedHaku,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var pon = Assert.IsType<PonResponse>(response);
        Assert.Equal(ponHandTiles, pon.HandTiles);
    }

    [Fact]
    public async Task 役ありシャンテンが維持される大明槓_DaiminkanResponseを返す()
    {
        // Arrange: 2 面子 + Haku 刻子 + 搭子 1 + 孤立 2 の 13 枚。
        // Haku 刻子が既に確定しているため、Haku 大明槓前後で翻牌経路のシャンテンは 1 から 1 に維持される。
        // 大明槓はシャンテン『維持』で採用する AI ポリシーで DaiminkanResponse が返ることを検証。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Sou1, TileKind.Sou2,
            TileKind.Ton, TileKind.Nan
        );
        var discardedHaku = new Tile(TileKind.Haku.Value * 4 + 3);
        var daiminkanHandTiles = ImmutableArray.Create(
            new Tile(TileKind.Haku.Value * 4),
            new Tile(TileKind.Haku.Value * 4 + 1),
            new Tile(TileKind.Haku.Value * 4 + 2));
        var candidates = new CandidateList(
        [
            new DaiminkanCandidate(daiminkanHandTiles),
        ]);
        var notification = new DahaiNotification(
            CreateView(hand),
            discardedHaku,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var daiminkan = Assert.IsType<DaiminkanResponse>(response);
        Assert.Equal(daiminkanHandTiles, daiminkan.HandTiles);
    }

    [Fact]
    public async Task 役ありシャンテンが進むチー_ChiResponseを返す()
    {
        // Arrange: 2 面子 (Pin/Sou) + Haku 対子 + Man1-Man2 搭子 + 孤立 Man4/Ton/Nan の 13 枚。
        // Man3 チーで Man1-2-3 順子が完成し、Yakuhai (Haku 対子) 経路の役ありシャンテンが進む。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man4,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Haku, TileKind.Haku,
            TileKind.Ton, TileKind.Nan
        );
        var discardedMan3 = new Tile(TileKind.Man3.Value * 4);
        var chiHandTiles = ImmutableArray.Create(
            new Tile(TileKind.Man1.Value * 4),
            new Tile(TileKind.Man2.Value * 4));
        var candidates = new CandidateList(
        [
            new ChiCandidate(chiHandTiles),
        ]);
        var notification = new DahaiNotification(
            CreateView(hand),
            discardedMan3,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var chi = Assert.IsType<ChiResponse>(response);
        Assert.Equal(chiHandTiles, chi.HandTiles);
    }

    [Fact]
    public async Task 自分テンパイ時_ChiCandidateあり_OkResponseを返す()
    {
        // Arrange: 3 面子 + Haku 対子 + Man5-Man6 搭子 = テンパイ手 (Man4/Man7 待ち)。
        // チー候補 (Man4 を Man5-Man6 で挟む) があってもテンパイなので副露しない。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Haku, TileKind.Haku,
            TileKind.Man5, TileKind.Man6
        );
        var discardedMan4 = new Tile(TileKind.Man4.Value * 4);
        var chiHandTiles = ImmutableArray.Create(
            new Tile(TileKind.Man5.Value * 4),
            new Tile(TileKind.Man6.Value * 4));
        var candidates = new CandidateList(
        [
            new ChiCandidate(chiHandTiles),
        ]);
        var notification = new DahaiNotification(
            CreateView(hand),
            discardedMan4,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert: テンパイ手牌なので副露せずテンパイ維持
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 他家リーチ中かつ高シャンテン_DaiminkanCandidateあり_OkResponseを返す()
    {
        // Arrange: 非テンパイ + 他家リーチ + 大明槓候補。シャンテン > 1 ならベタオリ継続で副露しない。
        // `役ありシャンテンが維持される大明槓_DaiminkanResponseを返す` の手牌から孤立を増やして高シャンテン化。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man5, TileKind.Man9,
            TileKind.Pin1, TileKind.Pin4, TileKind.Pin7,
            TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Sou2,
            TileKind.Ton, TileKind.Nan, TileKind.Pei
        );
        var discardedHaku = new Tile(TileKind.Haku.Value * 4 + 3);
        var daiminkanHandTiles = ImmutableArray.Create(
            new Tile(TileKind.Haku.Value * 4),
            new Tile(TileKind.Haku.Value * 4 + 1),
            new Tile(TileKind.Haku.Value * 4 + 2));
        var candidates = new CandidateList(
        [
            new DaiminkanCandidate(daiminkanHandTiles),
        ]);
        var notification = new DahaiNotification(
            CreateViewWithRiichiOpponent(hand, riichiOpponentIndex: 1),
            discardedHaku,
            new PlayerIndex(1),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 他家リーチ + 高シャンテンなら副露 (大明槓含む) しない
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 役ありシャンテンが進まないポン_OkResponseを返す()
    {
        // Arrange: 既にチー副露あり (対々和経路 INF, 門前 INF)。
        // 非役牌 Sha の対子のみで、Sha ポンは断么九経路を壊す (call に么九) ため全経路 INF となり、
        // 元のシャンテンより悪化する → ポン採用しない。
        var player = await CreateAIWithGameStartAsync();
        var chi = new Call(
            CallType.Chi,
            [
                new Tile(TileKind.Man2.Value * 4),
                new Tile(TileKind.Man3.Value * 4),
                new Tile(TileKind.Man4.Value * 4),
            ],
            new PlayerIndex(3),
            new Tile(TileKind.Man2.Value * 4)
        );
        var hand = BuildHand(
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Sha, TileKind.Sha,
            TileKind.Ton, TileKind.Pei
        );
        var discardedSha = new Tile(TileKind.Sha.Value * 4 + 2);
        var candidates = new CandidateList(
        [
            new PonCandidate([new Tile(TileKind.Sha.Value * 4), new Tile(TileKind.Sha.Value * 4 + 1)]),
        ]);
        var notification = new DahaiNotification(
            CreateViewWithOwnCalls(hand, [chi]),
            discardedSha,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    // ================= ヘルパー =================

    private static AI_v0_5_0_鳴き CreateAI(int seed = 42)
    {
        return new AI_v0_5_0_鳴き(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
    }

    private static async Task<AI_v0_5_0_鳴き> CreateAIWithGameStartAsync(int seed = 42)
    {
        var player = CreateAI(seed);
        var rules = new GameRules();
        var others = Enumerable.Range(1, 3)
            .Select(x => new AI_v0_5_0_鳴き(PlayerId.NewId(), new PlayerIndex(x), new Random(seed + x)));
        var playerList = new PlayerList([player, .. others]);
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));
        await player.OnGameStartAsync(notification, TestContext.Current.CancellationToken);
        return player;
    }

    private static Hand BuildHand(params TileKind[] kinds)
    {
        var countsByKind = new Dictionary<TileKind, int>();
        var tiles = new List<Tile>();
        foreach (var kind in kinds)
        {
            if (!countsByKind.TryGetValue(kind, out var copy)) { copy = 0; }
            countsByKind[kind] = copy + 1;
            tiles.Add(new Tile(kind.Value * 4 + copy));
        }
        return new Hand(tiles);
    }

    private static PlayerRoundView CreateView(Hand ownHand)
    {
        return BuildView(ownHand, new CallListArray(), otherRiichiIndex: null);
    }

    private static PlayerRoundView CreateViewWithRiichiOpponent(Hand ownHand, int riichiOpponentIndex)
    {
        return BuildView(ownHand, new CallListArray(), riichiOpponentIndex);
    }

    private static PlayerRoundView CreateViewWithOwnCalls(Hand ownHand, CallList ownCalls)
    {
        var callArray = new CallListArray();
        foreach (var call in ownCalls)
        {
            callArray = callArray.AddCall(new PlayerIndex(0), call);
        }
        return BuildView(ownHand, callArray, otherRiichiIndex: null);
    }

    private static PlayerRoundView BuildView(Hand ownHand, CallListArray callListArray, int? otherRiichiIndex)
    {
        var otherStatuses = new VisiblePlayerRoundStatus[3];
        for (var i = 0; i < 3; i++)
        {
            var playerIndex = new PlayerIndex(i + 1);
            var isRiichi = otherRiichiIndex == i + 1;
            var safe = isRiichi ? ImmutableHashSet<TileKind>.Empty : null;
            otherStatuses[i] = new VisiblePlayerRoundStatus(playerIndex, isRiichi, false, true, safe);
        }
        return new PlayerRoundView(
            ViewerIndex: new PlayerIndex(0),
            RoundWind: RoundWind.East,
            RoundNumber: new RoundNumber(0),
            Honba: new Honba(0),
            KyoutakuRiichiCount: new KyoutakuRiichiCount(0),
            TurnIndex: new PlayerIndex(0),
            PointArray: new PointArray(new Point(35000)),
            OwnHand: ownHand,
            CallListArray: callListArray,
            RiverArray: new RiverArray(),
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses: [.. otherStatuses],
            WallLiveRemaining: 70
        );
    }
}
