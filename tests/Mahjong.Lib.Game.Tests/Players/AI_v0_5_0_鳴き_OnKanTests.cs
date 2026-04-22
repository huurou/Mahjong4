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
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_5_0_鳴き_OnKanTests
{
    [Fact]
    public async Task ChankanRonCandidateあり_ChankanRonResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
        var call = new Call(
            CallType.Kakan,
            [new Tile(0), new Tile(1), new Tile(2), new Tile(3)],
            new PlayerIndex(3),
            new Tile(0)
        );
        var candidates = new CandidateList([new ChankanRonCandidate()]);
        var notification = new KanNotification(
            CreateView(new Hand(Enumerable.Range(0, 13).Select(x => new Tile(x * 4)))),
            call,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnKanAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<ChankanRonResponse>(response);
    }

    [Fact]
    public async Task RonCandidateあり_RonResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
        var call = new Call(
            CallType.Ankan,
            [new Tile(0), new Tile(1), new Tile(2), new Tile(3)],
            new PlayerIndex(3),
            null
        );
        var candidates = new CandidateList([new RonCandidate()]);
        var notification = new KanNotification(
            CreateView(new Hand(Enumerable.Range(0, 13).Select(x => new Tile(x * 4)))),
            call,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnKanAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RonResponse>(response);
    }

    [Fact]
    public async Task 候補なし_OkResponseを返す()
    {
        // Arrange
        var player = await CreateAIWithGameStartAsync();
        var call = new Call(
            CallType.Ankan,
            [new Tile(0), new Tile(1), new Tile(2), new Tile(3)],
            new PlayerIndex(3),
            null
        );
        var candidates = new CandidateList([]);
        var notification = new KanNotification(
            CreateView(new Hand(Enumerable.Range(0, 13).Select(x => new Tile(x * 4)))),
            call,
            new PlayerIndex(3),
            candidates,
            [new PlayerIndex(0)]
        );

        // Act
        var response = await player.OnKanAsync(notification, TestContext.Current.CancellationToken);

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
}
