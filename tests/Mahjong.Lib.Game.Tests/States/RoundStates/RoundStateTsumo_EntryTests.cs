using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateTsumo_EntryTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ツモ状態入場_Turnの手牌が十四枚になる()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());

        // Act
        RoundStateContextTestHelper.DriveResponseOk(context_);

        // Assert
        Assert.Equal(14, context_.Round.HandArray[context_.Round.Turn].Count());
        Assert.Contains(new Tile(83), context_.Round.HandArray[context_.Round.Turn]);
    }
}
