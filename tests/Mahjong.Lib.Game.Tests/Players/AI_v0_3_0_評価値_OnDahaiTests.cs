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
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_3_0_評価値_OnDahaiTests
{
    [Fact]
    public async Task RonCandidateあり_RonResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList([new RonCandidate()]);
        var notification = new DahaiNotification(CreateView(), new Tile(0), new PlayerIndex(1), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RonResponse>(response);
    }

    [Fact]
    public async Task Ron以外の候補のみ_OkResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList([new OkCandidate()]);
        var notification = new DahaiNotification(CreateView(), new Tile(0), new PlayerIndex(1), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OnKan_OkResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList([new OkCandidate()]);
        var kanCall = new Call(
            CallType.Ankan,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3)),
            new PlayerIndex(1),
            null
        );
        var notification = new KanNotification(CreateView(), kanCall, new PlayerIndex(1), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OnKan_RonCandidateあり_RonResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList([new RonCandidate()]);
        var kanCall = new Call(
            CallType.Kakan,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3)),
            new PlayerIndex(1),
            new Tile(3)
        );
        var notification = new KanNotification(CreateView(), kanCall, new PlayerIndex(1), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RonResponse>(response);
    }

    private static AI_v0_3_0_評価値 CreateAI()
    {
        return new AI_v0_3_0_評価値(PlayerId.NewId(), new PlayerIndex(0), new Random(42));
    }

    private static PlayerRoundView CreateView()
    {
        return new PlayerRoundView(
            new PlayerIndex(0),
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(35000)),
            new Hand(),
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
