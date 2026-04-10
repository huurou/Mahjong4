using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_EqualsTests
{
    [Fact]
    public void 同じ値の牌_等しいと判定される()
    {
        // Arrange & Act & Assert
        Assert.Equal(TileKind.Man1, TileKind.Man1);
        Assert.Equal(new TileKind(0), new TileKind(0));
        Assert.True(TileKind.Man1.Equals(new TileKind(0)));
    }

    [Fact]
    public void 異なる値の牌_等しくないと判定される()
    {
        // Arrange & Act & Assert
        Assert.NotEqual(TileKind.Man1, TileKind.Man2);
        Assert.NotEqual(new TileKind(0), new TileKind(1));
        Assert.False(TileKind.Man1.Equals(TileKind.Man2));
    }

    [Fact]
    public void Nullとの比較_等しくない()
    {
        // Arrange & Act & Assert
        Assert.False(TileKind.Man1.Equals(null));
    }
}
