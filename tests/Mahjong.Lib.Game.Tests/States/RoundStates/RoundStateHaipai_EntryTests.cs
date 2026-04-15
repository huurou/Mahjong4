using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateHaipai_EntryTests : IDisposable
{
    private readonly RoundStateContext context_ = new();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 配牌状態入場_各プレイヤーに十三枚ずつ配られる()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());

        // Act
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(context_);

        // Assert
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(0)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(1)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(2)].Count());
        Assert.Equal(13, context_.Round.HandArray[new PlayerIndex(3)].Count());
    }

    [Fact]
    public async Task 配牌状態入場_山のDrawnCountが五十二()
    {
        // Arrange
        context_.Init(RoundStateContextTestHelper.CreateRound());

        // Act
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(context_);

        // Assert
        Assert.Equal(52, context_.Round.Wall.DrawnCount);
    }
}
