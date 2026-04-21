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

public class AI_v0_3_0_評価値_OnKanTsumoTests
{
    [Fact]
    public async Task RinshanTsumoAgariCandidateあり_RinshanTsumoResponseを返す()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList(
        [
            new RinshanTsumoAgariCandidate(),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(0), false)])),
        ]);
        var notification = new KanTsumoNotification(CreateView(new Hand()), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<RinshanTsumoResponse>(response);
    }

    [Fact]
    public async Task 嶺上ツモ和了なしかつリーチ可能_KanTsumoDahaiResponseでIsRiichiがtrue()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, RiichiAvailable: true)]);
        var player = CreateAI();

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new KanTsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<KanTsumoDahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
        Assert.True(dahai.IsRiichi);
    }

    [Fact]
    public async Task 嶺上ツモ和了なしかつリーチ不可_KanTsumoDahaiResponseでIsRiichiがfalse()
    {
        // Arrange
        var hand = new Hand(Enumerable.Range(0, 14).Select(x => new Tile(x * 4)));
        var tileA = new Tile(0);
        var options = new DahaiOptionList([new DahaiOption(tileA, RiichiAvailable: false)]);
        var player = CreateAI();

        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new KanTsumoNotification(CreateView(hand), tileA, candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<KanTsumoDahaiResponse>(response);
        Assert.Equal(tileA.Id, dahai.Tile.Id);
        Assert.False(dahai.IsRiichi);
    }

    [Fact]
    public async Task DahaiCandidateなし_例外()
    {
        // Arrange
        var player = CreateAI();
        var candidates = new CandidateList([]);
        var notification = new KanTsumoNotification(CreateView(new Hand()), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var ex = await Record.ExceptionAsync(async () => await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    private static AI_v0_3_0_評価値 CreateAI()
    {
        return new AI_v0_3_0_評価値(PlayerId.NewId(), new PlayerIndex(0), new Random(42));
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
