using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Tiles;

public class Tile_IsYaochuuTests
{
    [Theory]
    [InlineData(0)]    // 1m
    [InlineData(32)]   // 9m (kind 8)
    [InlineData(36)]   // 1p (kind 9)
    [InlineData(68)]   // 9p (kind 17)
    [InlineData(72)]   // 1s (kind 18)
    [InlineData(104)]  // 9s (kind 26)
    [InlineData(108)]  // 東 (kind 27)
    [InlineData(132)]  // 中 (kind 33)
    public void 幺九牌_trueを返す(int id)
    {
        // Arrange
        var tile = new Tile(id);

        // Act & Assert
        Assert.True(tile.IsYaochuu);
    }

    [Theory]
    [InlineData(4)]    // 2m (kind 1)
    [InlineData(28)]   // 8m (kind 7)
    [InlineData(40)]   // 2p (kind 10)
    [InlineData(64)]   // 8p (kind 16)
    [InlineData(76)]   // 2s (kind 19)
    [InlineData(100)]  // 8s (kind 25)
    public void 中張牌_falseを返す(int id)
    {
        // Arrange
        var tile = new Tile(id);

        // Act & Assert
        Assert.False(tile.IsYaochuu);
    }
}
