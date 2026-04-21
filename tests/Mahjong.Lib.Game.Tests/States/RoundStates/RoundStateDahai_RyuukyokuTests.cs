using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateDahai_RyuukyokuTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    private void InitInDahaiState()
    {
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        context_.State = new RoundStateDahai();
    }

    [Fact]
    public void 四家立直成立_SuuchaRiichiで流局遷移()
    {
        // Arrange
        InitInDahaiState();
        var statusArray = context_.Round.PlayerRoundStatusArray;
        for (var i = 0; i < 4; i++)
        {
            var index = new PlayerIndex(i);
            statusArray = statusArray.SetStatus(index, statusArray[index] with { IsRiichi = true });
        }
        context_.Round = context_.Round with { PlayerRoundStatusArray = statusArray };

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        var state = Assert.IsType<RoundStateRyuukyoku>(context_.State);
        Assert.Equal(RyuukyokuType.SuuchaRiichi, state.EventArgs.Type);
    }

    [Fact]
    public void 四風連打成立_Suufonrendaで流局遷移()
    {
        // Arrange
        InitInDahaiState();
        var riverArray = context_.Round.RiverArray;
        // 東: Kind 27 (Tile.Id 108..111)
        for (var i = 0; i < 4; i++)
        {
            riverArray = riverArray.AddTile(new PlayerIndex(i), new Tile(108 + i));
        }
        context_.Round = context_.Round with { RiverArray = riverArray };

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        var state = Assert.IsType<RoundStateRyuukyoku>(context_.State);
        Assert.Equal(RyuukyokuType.Suufonrenda, state.EventArgs.Type);
    }

    [Fact]
    public void 四風連打と四家立直が同時成立_Suufonrenda優先()
    {
        // Arrange
        InitInDahaiState();
        var riverArray = context_.Round.RiverArray;
        var statusArray = context_.Round.PlayerRoundStatusArray;
        for (var i = 0; i < 4; i++)
        {
            var index = new PlayerIndex(i);
            riverArray = riverArray.AddTile(index, new Tile(108 + i));
            statusArray = statusArray.SetStatus(index, statusArray[index] with { IsRiichi = true });
        }
        context_.Round = context_.Round with
        {
            RiverArray = riverArray,
            PlayerRoundStatusArray = statusArray,
        };

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        var state = Assert.IsType<RoundStateRyuukyoku>(context_.State);
        Assert.Equal(RyuukyokuType.Suufonrenda, state.EventArgs.Type);
    }

    [Fact]
    public void 流局条件なし_ツモ状態へ遷移()
    {
        // Arrange: 通常の Haipai → OK → Dahai → OK のパス (流局無し)
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseDahai(context_, RoundStateContextTestHelper.PickTileToDahai(context_));

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.IsType<RoundStateTsumo>(context_.State);
    }

    [Fact]
    public void 三家和了応答_保留立直はキャンセルされリーチ棒が出ない()
    {
        // Arrange: 立直保留中のプレイヤーを設定、打牌状態から SanchaHou 応答を受信
        InitInDahaiState();
        var pendingIndex = new PlayerIndex(0);
        var statusArray = context_.Round.PlayerRoundStatusArray.SetStatus(
            pendingIndex,
            context_.Round.PlayerRoundStatusArray[pendingIndex] with { IsPendingRiichi = true }
        );
        context_.Round = context_.Round with { PlayerRoundStatusArray = statusArray };
        var initialKyoutaku = context_.Round.KyoutakuRiichiCount.Value;
        var initialPoint = context_.Round.PointArray[pendingIndex].Value;

        // Act
        RoundStateContextTestHelper.DriveResponseRyuukyoku(context_, RyuukyokuType.SanchaHou, []);

        // Assert
        var state = Assert.IsType<RoundStateRyuukyoku>(context_.State);
        Assert.Equal(RyuukyokuType.SanchaHou, state.EventArgs.Type);
        Assert.Equal(initialKyoutaku, context_.Round.KyoutakuRiichiCount.Value);
        Assert.Equal(initialPoint, context_.Round.PointArray[pendingIndex].Value);
        Assert.False(context_.Round.PlayerRoundStatusArray[pendingIndex].IsRiichi);
        Assert.False(context_.Round.PlayerRoundStatusArray[pendingIndex].IsPendingRiichi);
    }
}
