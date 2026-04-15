using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class PlayerIndex_NextTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 0)]
    public void 現在の値_次のプレイヤーが返る(int current, int expected)
    {
        // Arrange
        var playerIndex = new PlayerIndex(current);

        // Act
        var next = playerIndex.Next();

        // Assert
        Assert.Equal(expected, next.Value);
    }
}
