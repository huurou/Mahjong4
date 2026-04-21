using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Players;

public class FakePlayer_OnXxxTests
{
    [Fact]
    public async Task OnTsumo_Responder未設定_先頭のDahaiCandidateを打牌する既定応答を返す()
    {
        // Arrange
        var player = FakePlayer.Create(0);
        var tile = new Tile(0);
        var candidates = new CandidateList([new DahaiCandidate(new DahaiOptionList([new DahaiOption(tile, false)]))]);
        var notification = new TsumoNotification(CreateView(), tile, candidates, []);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tile, dahai.Tile);
    }

    [Fact]
    public async Task OnTsumo_Responder設定_設定された応答を返す()
    {
        // Arrange
        var expected = new TsumoAgariResponse();
        var player = new FakePlayer(PlayerId.NewId(), "F0", new PlayerIndex(0))
        {
            OnTsumo = (_, _) => expected,
        };
        var tile = new Tile(0);
        var notification = new TsumoNotification(CreateView(), tile, new CandidateList([new TsumoAgariCandidate()]), []);

        // Act
        var response = await player.OnTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(expected, response);
    }

    [Fact]
    public async Task OnDahai_Responder未設定_OkResponseを返す()
    {
        var player = FakePlayer.Create(0);
        var response = await player.OnDahaiAsync(new DahaiNotification(CreateView(), new Tile(0), new PlayerIndex(1), new CandidateList([new OkCandidate()]), []), TestContext.Current.CancellationToken);
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OnDahai_Responder設定_設定された応答を返す()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1));
        var player = new FakePlayer(PlayerId.NewId(), "F0", new PlayerIndex(0))
        {
            OnDahai = (_, _) => new PonResponse(tiles),
        };
        var notification = new DahaiNotification(CreateView(), new Tile(0), new PlayerIndex(1), new CandidateList([new OkCandidate()]), [new PlayerIndex(0)]);

        // Act
        var response = await player.OnDahaiAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var pon = Assert.IsType<PonResponse>(response);
        Assert.Equal(tiles, pon.HandTiles);
    }

    [Fact]
    public async Task OnKan_Responder未設定_OkResponseを返す()
    {
        var player = FakePlayer.Create(0);
        var call = new Call(CallType.Kakan, [new Tile(0), new Tile(1), new Tile(2), new Tile(3)], new PlayerIndex(0), new Tile(3));
        var response = await player.OnKanAsync(new KanNotification(CreateView(), call, new PlayerIndex(0), new CandidateList([new OkCandidate()]), []), TestContext.Current.CancellationToken);
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OnKanTsumo_Responder未設定_先頭のDahaiCandidateを打牌する既定応答を返す()
    {
        // Arrange
        var tile = new Tile(10);
        var candidates = new CandidateList([new DahaiCandidate(new DahaiOptionList([new DahaiOption(tile, false)]))]);
        var player = FakePlayer.Create(0);
        var notification = new KanTsumoNotification(CreateView(), tile, candidates, []);

        // Act
        var response = await player.OnKanTsumoAsync(notification, TestContext.Current.CancellationToken);

        // Assert
        var dahai = Assert.IsType<KanTsumoDahaiResponse>(response);
        Assert.Equal(tile, dahai.Tile);
    }

    [Fact]
    public async Task OnGameStart_既定_OkResponseを返す()
    {
        var player = FakePlayer.Create(0);
        var playerList = new PlayerList(PlayersTestHelper.CreateTestPlayers(4));
        var response = await player.OnGameStartAsync(new GameStartNotification(playerList, new GameRules(), new PlayerIndex(0)), TestContext.Current.CancellationToken);
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task OnHaipai_既定_OkResponseを返す()
    {
        var player = FakePlayer.Create(0);
        var response = await player.OnHaipaiAsync(new HaipaiNotification(CreateView(), []), TestContext.Current.CancellationToken);
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public async Task 通知受信_ReceivedNotificationsに記録される()
    {
        // Arrange
        var player = FakePlayer.Create(0);
        var haipai = new HaipaiNotification(CreateView(), []);
        var roundStart = new RoundStartNotification(RoundWind.East, new RoundNumber(0), new Honba(0), new PlayerIndex(0));

        // Act
        await player.OnRoundStartAsync(roundStart, TestContext.Current.CancellationToken);
        await player.OnHaipaiAsync(haipai, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, player.ReceivedNotifications.Count);
        Assert.Same(roundStart, player.ReceivedNotifications[0]);
        Assert.Same(haipai, player.ReceivedNotifications[1]);
    }

    [Fact]
    public async Task CancellationToken_Responderに伝播される()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken receivedCt = default;
        var player = new FakePlayer(PlayerId.NewId(), "F0", new PlayerIndex(0))
        {
            OnTsumo = (_, ct) => { receivedCt = ct; return new TsumoAgariResponse(); },
        };
        var notification = new TsumoNotification(CreateView(), new Tile(0), new CandidateList([new TsumoAgariCandidate()]), []);

        // Act
        await player.OnTsumoAsync(notification, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedCt);
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
