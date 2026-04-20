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

public class AI_v0_1_0_ランダム_OnKanTsumoTests
{
    [Fact]
    public async Task RinshanTsumoAgariCandidateあり_RinshanTsumoResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 1);
        var candidates = new CandidateList(
        [
            new RinshanTsumoAgariCandidate(),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(0), false)])),
        ]);
        var notification = new KanTsumoNotification(CreateView(), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RinshanTsumoResponse>(response);
    }

    [Fact]
    public async Task RinshanTsumoAgariCandidateなし_KanTsumoDahaiResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 1);
        var options = new DahaiOptionList(
        [
            new DahaiOption(new Tile(0), false),
            new DahaiOption(new Tile(4), false),
        ]);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new KanTsumoNotification(CreateView(), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<KanTsumoDahaiResponse>(response);
        Assert.False(dahai.IsRiichi);
        Assert.Contains(dahai.Tile, options.Select(x => x.Tile));
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
