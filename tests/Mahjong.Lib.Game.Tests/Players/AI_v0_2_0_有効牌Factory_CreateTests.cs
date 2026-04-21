using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_2_0_有効牌Factory_CreateTests
{
    [Fact]
    public void 生成されるPlayerは指定の表示名を持つ()
    {
        // Arrange
        var factory = new AI_v0_2_0_有効牌Factory(seed: 42);

        // Act
        var player = factory.Create(new PlayerIndex(0), PlayerId.NewId());

        // Assert
        Assert.Equal("ver.0.2.0 有効牌", player.DisplayName);
    }

    [Fact]
    public void 生成されるFactoryのDisplayNameが定数と一致する()
    {
        // Arrange
        var factory = new AI_v0_2_0_有効牌Factory(seed: 0);

        // Assert
        Assert.Equal(AI_v0_2_0_有効牌.DISPLAY_NAME, factory.DisplayName);
    }
}
