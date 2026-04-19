using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateHaipai_EntryTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 配牌状態入場_各プレイヤーに十三枚ずつ配られる()
    {
        // Arrange & Act
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());

        // Assert
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(0)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(1)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(2)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(3)].Count());
    }

    [Fact]
    public void 配牌状態入場_山のDrawnCountが五十二()
    {
        // Arrange & Act
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());

        // Assert
        Assert.Equal(52, context_.Round.Wall.DrawnCount);
    }
}
