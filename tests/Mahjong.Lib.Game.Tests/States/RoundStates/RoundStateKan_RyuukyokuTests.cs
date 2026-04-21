using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKan_RyuukyokuTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Call MakeAnkan(int kindId)
    {
        var baseId = kindId * 4;
        return new Call(
            CallType.Ankan,
            [new(baseId), new(baseId + 1), new(baseId + 2), new(baseId + 3)],
            new PlayerIndex(0),
            null
        );
    }

    /// <summary>
    /// RoundStateKan (槍槓応答待ち) に直接遷移させる。嶺上ツモ前の四槓流れ判定をテストするため
    /// </summary>
    private void PrepareKanState()
    {
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        context_.State = new RoundStateKan(CallType.Ankan, kanTiles);
    }

    [Fact]
    public void 四槓流れ成立_嶺上ツモ前にSuukaikanで流局遷移()
    {
        // Arrange
        PrepareKanState();
        var beforeLiveRemaining = context_.Round.Wall.LiveRemaining;
        var beforeRinshanCount = context_.Round.Wall.RinshanDrawnCount;
        var callListArray = context_.Round.CallListArray;
        // 2人 × 2槓 = 合計4槓 (declarers=2)
        callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(0));
        callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(1));
        callListArray = callListArray.AddCall(new PlayerIndex(1), MakeAnkan(2));
        callListArray = callListArray.AddCall(new PlayerIndex(1), MakeAnkan(3));
        context_.Round = context_.Round with { CallListArray = callListArray };

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        var state = Assert.IsType<RoundStateRyuukyoku>(context_.State);
        Assert.Equal(RyuukyokuType.Suukaikan, state.EventArgs.Type);
        // 嶺上ツモ前に流局へ抜けるため、嶺上牌と壁は消費されない
        Assert.Equal(beforeLiveRemaining, context_.Round.Wall.LiveRemaining);
        Assert.Equal(beforeRinshanCount, context_.Round.Wall.RinshanDrawnCount);
    }

    [Fact]
    public void 同一プレイヤー四槓子_流局せず嶺上ツモ状態に遷移()
    {
        // Arrange
        PrepareKanState();
        var callListArray = context_.Round.CallListArray;
        // 1 人 × 4槓 = 合計4槓 (declarers=1 → 四槓子狙いなので流局させない)
        for (var kind = 0; kind < 4; kind++)
        {
            callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(kind));
        }
        context_.Round = context_.Round with { CallListArray = callListArray };

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.IsType<RoundStateKanTsumo>(context_.State);
    }
}
