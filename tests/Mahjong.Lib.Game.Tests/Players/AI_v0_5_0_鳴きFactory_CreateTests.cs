using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_5_0_鳴きFactory_CreateTests
{
    [Fact]
    public void Create_PlayerIndex0から3まで_正しい席のプレイヤーを返す()
    {
        // Arrange
        var factory = new AI_v0_5_0_鳴きFactory(seed: 123);

        // Act & Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var id = PlayerId.NewId();
            var player = factory.Create(new PlayerIndex(i), id);
            Assert.IsType<AI_v0_5_0_鳴き>(player);
            Assert.Equal(id, player.PlayerId);
            Assert.Equal(i, player.PlayerIndex.Value);
        }
    }

    [Fact]
    public void Create_表示名が指定通り()
    {
        // Arrange
        var factory = new AI_v0_5_0_鳴きFactory(seed: 1);

        // Act
        var player = factory.Create(new PlayerIndex(0), PlayerId.NewId());

        // Assert
        Assert.Equal(AI_v0_5_0_鳴き.DISPLAY_NAME, player.DisplayName);
    }

    [Fact]
    public void DisplayName_は_ver050_鳴き()
    {
        Assert.Equal("ver.0.5.0 鳴き", AI_v0_5_0_鳴き.DISPLAY_NAME);
    }
}
