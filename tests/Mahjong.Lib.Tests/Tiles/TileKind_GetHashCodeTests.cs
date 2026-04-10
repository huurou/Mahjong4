using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_GetHashCodeTests
{
    [Fact]
    public void 同じ値の牌_同じハッシュコードを返す()
    {
        // Arrange & Act & Assert
        Assert.Equal(TileKind.Man1.GetHashCode(), new TileKind(0).GetHashCode());
        Assert.Equal(TileKind.Man5.GetHashCode(), new TileKind(4).GetHashCode());
    }

    [Fact]
    public void 異なる値の牌_異なるハッシュコードを返す()
    {
        // Arrange & Act & Assert
        Assert.NotEqual(TileKind.Man1.GetHashCode(), TileKind.Man2.GetHashCode());
    }
}
