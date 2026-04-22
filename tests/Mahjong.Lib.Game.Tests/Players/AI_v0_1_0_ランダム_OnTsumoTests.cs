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

public class AI_v0_1_0_ランダム_OnTsumoTests
{
    [Fact]
    public async Task TsumoAgariCandidateあり_TsumoAgariResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 42);
        var candidates = new CandidateList(
        [
            new TsumoAgariCandidate(),
            new DahaiCandidate(new DahaiOptionList([new DahaiOption(new Tile(0), false)])),
        ]);
        var notification = new TsumoNotification(CreateView(), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.IsType<TsumoAgariResponse>(response);
    }

    [Fact]
    public async Task TsumoAgariCandidateなし_DahaiResponseを返す()
    {
        // Arrange
        var player = CreateAI_v0_1_0_ランダム(seed: 42);
        var options = new DahaiOptionList(
        [
            new DahaiOption(new Tile(0), false),
            new DahaiOption(new Tile(4), false),
            new DahaiOption(new Tile(8), false),
        ]);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.False(dahai.IsRiichi);
        Assert.Contains(dahai.Tile, options.Select(x => x.Tile));
    }

    [Fact]
    public async Task 同一シードで同一応答列()
    {
        // Arrange
        var options = new DahaiOptionList(
        [
            new DahaiOption(new Tile(0), false),
            new DahaiOption(new Tile(4), false),
            new DahaiOption(new Tile(8), false),
            new DahaiOption(new Tile(12), false),
            new DahaiOption(new Tile(16), false),
        ]);
        var candidates = new CandidateList([new DahaiCandidate(options)]);
        var notification = new TsumoNotification(CreateView(), new Tile(0), candidates, [new PlayerIndex(0)]);

        // Act: 同一シードで 2 個の player を作り、同じ入力に対して同じ応答が返ることを確認
        var player1 = CreateAI_v0_1_0_ランダム(seed: 100);
        var player2 = CreateAI_v0_1_0_ランダム(seed: 100);
        var responses1 = new List<Tile>();
        var responses2 = new List<Tile>();
        for (var i = 0; i < 10; i++)
        {
            var r1 = (DahaiResponse)await player1.OnTsumoAsync(notification, TestContext.Current.CancellationToken);
            var r2 = (DahaiResponse)await player2.OnTsumoAsync(notification, TestContext.Current.CancellationToken);
            responses1.Add(r1.Tile);
            responses2.Add(r2.Tile);
        }

        // Assert
        Assert.Equal(responses1, responses2);
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
