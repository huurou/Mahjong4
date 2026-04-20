using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_1_0_ランダムFactory_CreateTests
{
    [Fact]
    public void AI_v0_1_0_ランダムを返す()
    {
        // Arrange
        var factory = new AI_v0_1_0_ランダムFactory(seed: 42);
        var id = PlayerId.NewId();

        // Act
        var player = factory.Create(new PlayerIndex(0), id);

        // Assert
        Assert.IsType<AI_v0_1_0_ランダム>(player);
        Assert.Equal(id, player.PlayerId);
        Assert.Equal(AI_v0_1_0_ランダム.DISPLAY_NAME, player.DisplayName);
        Assert.Equal(new PlayerIndex(0), player.PlayerIndex);
    }

    [Fact]
    public void DisplayNameプロパティがAIクラス定数と一致()
    {
        // Arrange
        var factory = new AI_v0_1_0_ランダムFactory(seed: 42);

        // Act
        var displayName = factory.DisplayName;

        // Assert
        Assert.Equal("ver.0.1.0 ランダム", displayName);
        Assert.Equal(AI_v0_1_0_ランダム.DISPLAY_NAME, displayName);
    }

    [Fact]
    public void 異なるPlayerIndexごとに異なるRandomを注入()
    {
        // Arrange
        var factory = new AI_v0_1_0_ランダムFactory(seed: 42);
        var id0 = PlayerId.NewId();
        var id1 = PlayerId.NewId();

        // Act
        var player0 = factory.Create(new PlayerIndex(0), id0);
        var player1 = factory.Create(new PlayerIndex(1), id1);

        // Assert: 両方とも AI_v0_1_0_ランダム
        Assert.IsType<AI_v0_1_0_ランダム>(player0);
        Assert.IsType<AI_v0_1_0_ランダム>(player1);
        Assert.NotEqual(player0.PlayerIndex, player1.PlayerIndex);
    }
}
