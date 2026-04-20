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

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

// KanTsumo (嶺上ツモ) 局面の 2 段階ディスパッチ (pendingAfterKanTsumoResponse_) を通る
// 4 系統: RinshanTsumo 和了 / KanTsumoDahai / KanTsumoAnkan / KanTsumoKakan のうち和了/打牌フローを検証する
// Ankan / Kakan 連鎖は設定が複雑なため基本 2 系統に留める
public class RoundStateContext_RuntimeKanTsumoFlowTests
{
    // 親の既定 Haipai (末尾から 4 枚): Tile(135)/Tile(134)/Tile(133)/Tile(132) いずれも Kind 33 (中)
    // → 親は 1 巡目ツモ直後に Kind 33 の暗槓が可能
    // 暗槓後の嶺上牌 (tiles_[1]): Tile(1) Kind 0 (m1)

    private static RoundStateContext CreateContext(IEnumerable<Player> players)
    {
        return RoundStateContextRuntimeTestHelper.CreateContext(
            players,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundStateContext>.Instance
        );
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
        using var ctx = CreateContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, TimeSpan.FromSeconds(15));

        // Assert: 嶺上打牌で Ryuukyoku へ進行したこと + 親が KanTsumoNotification を受信していたこと
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
