using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_3_0_評価値Factory_CreateTests
{
    [Fact]
    public void 生成されるPlayerは指定の表示名を持つ()
    {
        // Arrange
        var factory = new AI_v0_3_0_評価値Factory(seed: 42);

        // Act
        var player = factory.Create(new PlayerIndex(0), PlayerId.NewId());

        // Assert
        Assert.Equal("ver.0.3.0 評価値", player.DisplayName);
    }

    [Fact]
    public void 生成されるFactoryのDisplayNameが定数と一致する()
    {
        // Arrange
        var factory = new AI_v0_3_0_評価値Factory(seed: 0);

        // Assert
        Assert.Equal(AI_v0_3_0_評価値.DISPLAY_NAME, factory.DisplayName);
    }
}
