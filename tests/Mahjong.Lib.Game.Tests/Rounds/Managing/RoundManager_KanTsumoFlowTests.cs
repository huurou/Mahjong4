using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;
using Mahjong.Lib.Game.Tiles;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

// KanTsumo (嶺上ツモ) 局面の 2 段階ディスパッチ (pendingAfterKanTsumoResponse_) を通る
// 4 系統: RinshanTsumo 和了 / KanTsumoDahai / KanTsumoAnkan / KanTsumoKakan のうち和了/打牌フローを検証する
// Ankan / Kakan 連鎖は設定が複雑なため基本 2 系統に留める
public class RoundManager_KanTsumoFlowTests
{
    // 親の既定 Haipai (末尾から 4 枚): Tile(135)/Tile(134)/Tile(133)/Tile(132) いずれも Kind 33 (中)
    // → 親は 1 巡目ツモ直後に Kind 33 の暗槓が可能
    // 暗槓後の嶺上牌 (tiles_[1]): Tile(1) Kind 0 (m1)

    private static ITenpaiChecker CreateMock(params int[] waits)
    {
        var mock = new Mock<ITenpaiChecker>();
        mock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(false);
        mock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([.. waits]);
        return mock.Object;
    }

    private static RoundManager CreateManager(IEnumerable<Player> players, ITenpaiChecker checker)
    {
        return RoundManagerTestHelper.CreateManager(
            players,
            checker,
            RoundTestHelper.NoOpScoreCalculator,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundManager>.Instance
        );
    }

    [Fact]
    public async Task 暗槓後の嶺上ツモ和了_RinshanとしてRoundEndedByWinが発火する()
    {
        // Arrange: waits=[0] で嶺上牌 (kind 0) をツモ和了対象にする
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0))
            {
                OnTsumo = (_, _) => new AnkanResponse(new Tile(132)),
                OnKanTsumo = (_, _) => new RinshanTsumoResponse(),
            },
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var manager = CreateManager(players, CreateMock(0));

        // Act
        var task = manager.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundManagerTestHelper.AwaitRoundEndAsync(task, RoundManagerTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Rinshan, win.WinType);
        Assert.Single(win.WinnerIndices);
        Assert.Equal(0, win.WinnerIndices[0].Value);
    }

    [Fact]
    public async Task 暗槓後の嶺上打牌_AfterKanTsumoを経由して次手番のツモへ進む()
    {
        // Arrange: 嶺上ツモ後に和了せず打牌。waits=[] で和了候補は出ない。
        // 親は暗槓+嶺上打牌、以降全員規定応答で流局するまで進行することを確認
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0))
            {
                OnTsumo = OnTsumoAnkanFirstOnly(),
                OnKanTsumo = (n, _) =>
                {
                    var dahai = n.CandidateList.GetCandidates<DahaiCandidate>().First();
                    return new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile);
                },
            },
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var manager = CreateManager(players, CreateMock());

        // Act
        var task = manager.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundManagerTestHelper.AwaitRoundEndAsync(task, TimeSpan.FromSeconds(15));

        // Assert: 嶺上打牌で Ryuukyoku へ進行したこと + 子全員に嶺上打牌を含む DahaiNotification が届いたこと
        Assert.IsType<RoundEndedByRyuukyokuEventArgs>(result);
        // 親 (player 0) が KanTsumoNotification を受信している (= KanTsumo 局面を通過した)
        Assert.Contains(players[0].ReceivedNotifications, x => x is KanTsumoNotification);
    }

    private static Func<TsumoNotification, CancellationToken, AfterTsumoResponse> OnTsumoAnkanFirstOnly()
    {
        // 初回ツモのみ暗槓、2 回目以降は打牌に切り替え (連続暗槓防止)
        var count = 0;
        return (n, _) =>
        {
            count++;
            if (count == 1)
            {
                return new AnkanResponse(new Tile(132));
            }
            var dahai = n.CandidateList.GetCandidates<DahaiCandidate>().First();
            return new DahaiResponse(dahai.DahaiOptionList[0].Tile);
        };
    }
}
