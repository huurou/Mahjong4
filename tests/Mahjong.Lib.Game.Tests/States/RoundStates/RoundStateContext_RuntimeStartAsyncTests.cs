using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_RuntimeStartAsyncTests
{
    [Fact]
    public async Task 全員規定応答_Wall消尽で荒牌平局が発生する()
    {
        // Arrange
        var players = RoundStateContextRuntimeTestHelper.CreateFakePlayers();
        using var ctx = RoundStateContextRuntimeTestHelper.CreatePermissiveContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, TimeSpan.FromSeconds(15));

        // Assert
        var ryu = Assert.IsType<RoundEndedByRyuukyokuEventArgs>(result);
        Assert.Equal(RyuukyokuType.KouhaiHeikyoku, ryu.Type);
    }

    [Fact]
    public async Task 配牌完了時_全プレイヤーにHaipaiNotificationが送信される()
    {
        // Arrange
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0)) { OnTsumo = (_, _) => new TsumoAgariResponse() },
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreatePermissiveContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, RoundStateContextRuntimeTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        Assert.All(players, p => Assert.Contains(p.ReceivedNotifications, x => x is HaipaiNotification));
    }

    [Fact]
    public async Task 親の九種九牌応答_途中流局が発生する()
    {
        // Arrange: 親の Haipai に幺九 13 種を仕込んで KyuushuKyuuhaiCandidate を成立させる
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0)) { OnTsumo = (_, _) => new KyuushuKyuuhaiResponse() },
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreateDefaultContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRoundWithDealerKyuushuHand(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, RoundStateContextRuntimeTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var ryu = Assert.IsType<RoundEndedByRyuukyokuEventArgs>(result);
        Assert.Equal(RyuukyokuType.KyuushuKyuuhai, ryu.Type);
    }
}
