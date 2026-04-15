using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Hands;

public class Hand_RemoveTileTests
{
    [Fact]
    public void 存在する牌_削除された新しいHandが返される()
    {
        // Arrange
        var hand = new Hand([new Tile(0), new Tile(1), new Tile(2)]);

        // Act
        var result = hand.RemoveTile(new Tile(1));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(new Tile(1), result);
    }

    [Fact]
    public void 存在しない牌_ArgumentExceptionが発生する()
    {
        // Arrange
        var hand = new Hand([new Tile(0), new Tile(1)]);

        // Act
        var ex = Record.Exception(() => hand.RemoveTile(new Tile(2)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
