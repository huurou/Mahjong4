using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class RoundManager_TimeoutTests
{
    [Fact]
    public async Task 他家プレイヤーがキャンセル例外_DefaultResponseFactoryのOK応答で継続進行する()
    {
        // Arrange: player[2] の OnDahai が OperationCanceledException を投げる
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0)) { OnTsumo = TsumoAgariAfterFirstDahai() },
            FakePlayer.Create(1),
            new(PlayerId.NewId(), "F2", new PlayerIndex(2))
            {
                OnDahaiHandler = (_, _) => throw new OperationCanceledException(),
            },
            FakePlayer.Create(3),
        };
        using var manager = RoundManagerTestHelper.CreatePermissiveManager(players);

        // Act: 例外は DefaultResponseFactory で OkResponse に置換され進行継続。2 巡目ツモでツモ和了して終了する
        var task = manager.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundManagerTestHelper.AwaitRoundEndAsync(task, RoundManagerTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Tsumo, win.WinType);
    }

    [Fact]
    public async Task 他家プレイヤーが一般例外_DefaultResponseFactoryで既定応答に置換される()
    {
        // Arrange
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0)) { OnTsumo = TsumoAgariAfterFirstDahai() },
            new(PlayerId.NewId(), "F1", new PlayerIndex(1))
            {
                OnDahai = (_, _) => throw new InvalidOperationException("failure"),
            },
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var manager = RoundManagerTestHelper.CreatePermissiveManager(players);

        // Act
        var task = manager.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundManagerTestHelper.AwaitRoundEndAsync(task, RoundManagerTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Tsumo, win.WinType);
    }

    private static Func<TsumoNotification, CancellationToken, AfterTsumoResponse> TsumoAgariAfterFirstDahai()
    {
        var count = 0;
        return (notification, _) =>
        {
            count++;
            if (count >= 2)
            {
                return new TsumoAgariResponse();
            }
            var dahai = notification.CandidateList.GetCandidates<DahaiCandidate>().First();
            return new DahaiResponse(dahai.DahaiOptionList[0].Tile);
        };
    }
}
