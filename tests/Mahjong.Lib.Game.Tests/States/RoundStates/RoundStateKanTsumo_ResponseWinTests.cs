using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKanTsumo_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 嶺上和了応答_和了状態に遷移する()
    {
        // Arrange: 親の手牌 = m5×4 (暗槓用) + 嶺上 m1 引きで和了する 10 枚
        // 連番ウォールの嶺上1手目は Tile(1) (m1) → 嶺上ツモ後 m1m2m3 + p234 + s234 + s5s5 で 3面子+雀頭+暗槓 = 嶺上開花和了
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.InjectRinshanAgariScenario(context_);
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Act: Rinshan で和了
        RoundStateContextTestHelper.DriveRinshanWin(context_);

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void ツモ和了応答_例外で和了状態に遷移しない()
    {
        // Arrange: 槓ツモ状態へ遷移
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Act: Tsumo (不正) を渡す
        var ex = Record.Exception(() => RoundStateContextTestHelper.DriveTsumoWin(context_));

        // Assert: 例外で遷移しない
        Assert.NotNull(ex);
        Assert.IsType<RoundStateKanTsumo>(context_.State);
    }
}
