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

public class AI_v0_5_0_鳴き_OnTsumoTests
{
    [Fact]
    public async Task TsumoAgariCandidateあり_TsumoAgariResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
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
    public async Task 通常手牌_DahaiResponseを返す()
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

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
    }

    [Fact]
    public async Task 役ありシャンテン維持する暗槓候補_AnkanResponseを返す()
    {
        // Arrange: 3 面子 + Haku 4 枚 + Ton 孤 = 14 枚。Haku 暗槓前後で Yakuhai 経路の役ありシャンテンは維持される。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Haku, TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Ton
        );
        var ankanTiles = ImmutableArray.Create(
            new Tile(TileKind.Haku.Value * 4),
            new Tile(TileKind.Haku.Value * 4 + 1),
            new Tile(TileKind.Haku.Value * 4 + 2),
            new Tile(TileKind.Haku.Value * 4 + 3));
        var candidates = new CandidateList(
        [
            new AnkanCandidate(ankanTiles),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(TileKind.Ton.Value * 4), false)])),
        ]);
        var notification = new TsumoNotification(
            CreateView(hand),
            new Tile(TileKind.Haku.Value * 4 + 3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var ankan = Assert.IsType<AnkanResponse>(response);
        Assert.Equal(TileKind.Haku, ankan.Tile.Kind);
    }

    [Fact]
    public async Task 他家リーチ中かつ非テンパイ_暗槓候補あり_DahaiResponseを返す()
    {
        // Arrange: 上記と同じ Haku 4 枚手牌だが、他家リーチ中 & 自非テンパイ (シャンテン > 0) なのでカンしない。
        // 非テンパイ化のため手牌の面子を崩して孤立牌を増やす。
        var player = await CreateAIWithGameStartAsync();
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man5, TileKind.Man9,
            TileKind.Pin1, TileKind.Pin4, TileKind.Pin7,
            TileKind.Sou2, TileKind.Sou5, TileKind.Sou8,
            TileKind.Haku, TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Ton
        );
        var ankanTiles = ImmutableArray.Create(
            new Tile(TileKind.Haku.Value * 4),
            new Tile(TileKind.Haku.Value * 4 + 1),
            new Tile(TileKind.Haku.Value * 4 + 2),
            new Tile(TileKind.Haku.Value * 4 + 3));
        var candidates = new CandidateList(
        [
            new AnkanCandidate(ankanTiles),
            new DahaiCandidate(new DahaiOptionList(
            [
                new DahaiOption(new Tile(TileKind.Ton.Value * 4), false),
                new DahaiOption(new Tile(TileKind.Man1.Value * 4), false),
                new DahaiOption(new Tile(TileKind.Man9.Value * 4), false),
            ])),
        ]);
        var notification = new TsumoNotification(
            CreateViewWithRiichiOpponent(hand, riichiOpponentIndex: 1),
            new Tile(TileKind.Haku.Value * 4 + 3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 他家リーチ中はカンを避けて DahaiResponse を返す
        Assert.IsType<DahaiResponse>(response);
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

    private static PlayerRoundView CreateView(Hand ownHand)
    {
        return BuildView(ownHand, otherRiichiIndex: null);
    }

    private static PlayerRoundView CreateViewWithRiichiOpponent(Hand ownHand, int riichiOpponentIndex)
    {
        return BuildView(ownHand, riichiOpponentIndex);
    }

    private static PlayerRoundView BuildView(Hand ownHand, int? otherRiichiIndex)
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
            CallListArray: new CallListArray(),
            RiverArray: new RiverArray(),
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses: [.. otherStatuses],
            WallLiveRemaining: 70
        );
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
}
