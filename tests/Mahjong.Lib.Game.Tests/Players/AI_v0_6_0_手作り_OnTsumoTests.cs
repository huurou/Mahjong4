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

public class AI_v0_6_0_手作り_OnTsumoTests
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
        // Arrange: 14 種 1 枚ずつの完全バラバラ手牌 (役ありシャンテン > 2 でフォールバックルートが走る)
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
    public async Task テンパイ手牌_評価値ベース打牌ルートを通る_DahaiResponseを返す()
    {
        // Arrange: Man1-9 (3 順子) + Chun×3 (刻子) + Pin5 + Pin9 の 14 枚テンパイ
        // 役ありシャンテン ≤ 2 なので評価値ベースルート (SelectDahaiByHandShapeEvaluation) が走る
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Man4, TileKind.Man5, TileKind.Man6,
            TileKind.Man7, TileKind.Man8, TileKind.Man9,
            TileKind.Chun, TileKind.Chun, TileKind.Chun,
            TileKind.Pin5,
            TileKind.Pin9
        );
        var pin9Tile = new Tile(TileKind.Pin9.Value * 4);
        var pin5Tile = new Tile(TileKind.Pin5.Value * 4);
        var options = new DahaiOptionList(
        [
            new DahaiOption(pin9Tile, false),
            new DahaiOption(pin5Tile, false),
        ]);
        var player = await CreateAIWithGameStartAsync();
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), pin9Tile, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert: 評価値ルートが例外なく完走し、2 候補のいずれかの DahaiResponse を返すこと
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Contains(dahai.Tile.Id, new[] { pin9Tile.Id, pin5Tile.Id });
    }

    [Fact]
    public async Task 同一シード_同じ打牌を返す()
    {
        // Arrange: v0.5.0 までと同様の決定論的再現性テスト
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, false)]);
        var player1 = await CreateAIWithGameStartAsync(seed: 42);
        var player2 = await CreateAIWithGameStartAsync(seed: 42);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response1 = await player1.OnTsumoAsync(notification, TestContext.Current.CancellationToken);
        var response2 = await player2.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai1 = Assert.IsType<DahaiResponse>(response1);
        var dahai2 = Assert.IsType<DahaiResponse>(response2);
        Assert.Equal(dahai1.Tile.Id, dahai2.Tile.Id);
    }

    // ================= ヘルパー =================

    private static AI_v0_6_0_手作り CreateAI(int seed = 42)
    {
        return new AI_v0_6_0_手作り(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
    }

    private static async Task<AI_v0_6_0_手作り> CreateAIWithGameStartAsync(int seed = 42)
    {
        var player = CreateAI(seed);
        var rules = new GameRules();
        var others = Enumerable.Range(1, 3)
            .Select(x => new AI_v0_6_0_手作り(PlayerId.NewId(), new PlayerIndex(x), new Random(seed + x)));
        var playerList = new PlayerList([player, .. others]);
        var notification = new GameStartNotification(playerList, rules, new PlayerIndex(0));
        await player.OnGameStartAsync(notification, TestContext.Current.CancellationToken);
        return player;
    }

    private static PlayerRoundView CreateView(Hand ownHand)
    {
        var otherStatuses = new VisiblePlayerRoundStatus[3];
        for (var i = 0; i < 3; i++)
        {
            otherStatuses[i] = new VisiblePlayerRoundStatus(new PlayerIndex(i + 1), false, false, true, null);
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
