using System.Collections.Immutable;
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

public class AI_v0_1_0_ランダム_OnDahaiTests
{
    [Fact]
    public async Task RonCandidateあり_RonResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 1);
        var candidates = new CandidateList([new RonCandidate(), new OkCandidate()]);
        var notification = new DahaiNotification(
            CreateView(), new Tile(0), new PlayerIndex(3), candidates, [new PlayerIndex(1), new PlayerIndex(2), new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RonResponse>(response);
    }

    [Fact]
    public async Task 副露候補だけ_OkResponseを返す_副露しない()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 1);
        var candidates = new CandidateList(
        [
            new PonCandidate([new Tile(0), new Tile(1)]),
            new OkCandidate(),
        ]);
        var notification = new DahaiNotification(
            CreateView(), new Tile(0), new PlayerIndex(3), candidates, [new PlayerIndex(1), new PlayerIndex(2), new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OkCandidateのみ_OkResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 1);
        var candidates = new CandidateList([new OkCandidate()]);
        var notification = new DahaiNotification(
            CreateView(), new Tile(0), new PlayerIndex(3), candidates, [new PlayerIndex(1), new PlayerIndex(2), new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<OkResponse>(response);
    }

    private static AI_v0_1_0_ランダム CreateAI_v0_1_0_ランダム(int seed)
    {
        return new AI_v0_1_0_ランダム(PlayerId.NewId(), new PlayerIndex(0), new Random(seed));
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
            new OwnRoundStatus(false, false, false, true, false, false, false, null),
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ],
            70
        );
    }
}
