using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_6_0_手作りFactory_CreateTests
{
    [Fact]
    public void Create_PlayerIndex0から3まで_正しい席のプレイヤーを返す()
    {
        // Arrange
        var factory = new AI_v0_6_0_手作りFactory(seed: 123);

        // Act & Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var id = PlayerId.NewId();
            var player = factory.Create(new PlayerIndex(i), id);
            Assert.IsType<AI_v0_6_0_手作り>(player);
            Assert.Equal(id, player.PlayerId);
            Assert.Equal(i, player.PlayerIndex.Value);
        }
    }

    [Fact]
    public void Create_表示名が指定通り()
    {
        // Arrange
        var factory = new AI_v0_6_0_手作りFactory(seed: 1);

        // Act
        var player = factory.Create(new PlayerIndex(0), PlayerId.NewId());

        // Assert
        Assert.Equal(AI_v0_6_0_手作り.DISPLAY_NAME, player.DisplayName);
    }

    [Fact]
    public void DisplayName_は_ver060_手作り()
    {
        Assert.Equal("ver.0.6.0 手作り", AI_v0_6_0_手作り.DISPLAY_NAME);
    }
}
