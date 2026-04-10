using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_ComparisonOperatorTests
{
    [Fact]
    public void 小なり_左が小さい場合true()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Man1 < TileKind.Man2);
        Assert.True(TileKind.Man1 < TileKind.Man5);
        Assert.True(TileKind.Man5 < TileKind.Man9);
        Assert.True(TileKind.Man9 < TileKind.Pin1);
        Assert.True(TileKind.Pin1 < TileKind.Chun);
        Assert.False(TileKind.Man2 < TileKind.Man1);
        Assert.False(TileKind.Man1 < TileKind.Man1);

        // 同種牌での比較
        Assert.True(TileKind.Man1 < TileKind.Man9);
        Assert.True(TileKind.Pin1 < TileKind.Pin9);
        Assert.True(TileKind.Sou1 < TileKind.Sou9);
        Assert.True(TileKind.Ton < TileKind.Chun);
    }

    [Fact]
    public void 小なりイコール_左が小さいか等しい場合true()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Man1 <= TileKind.Man2);
        Assert.True(TileKind.Man1 <= TileKind.Man5);
        Assert.True(TileKind.Man5 <= TileKind.Man9);
        Assert.True(TileKind.Man1 <= TileKind.Man1);
        Assert.False(TileKind.Man2 <= TileKind.Man1);
        Assert.False(TileKind.Man5 <= TileKind.Man2);
    }

    [Fact]
    public void 大なり_左が大きい場合true()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Man2 > TileKind.Man1);
        Assert.True(TileKind.Man5 > TileKind.Man1);
        Assert.True(TileKind.Man9 > TileKind.Man5);
        Assert.True(TileKind.Pin1 > TileKind.Man9);
        Assert.True(TileKind.Chun > TileKind.Pin1);
        Assert.False(TileKind.Man1 > TileKind.Man2);
        Assert.False(TileKind.Man1 > TileKind.Man1);
    }

    [Fact]
    public void 大なりイコール_左が大きいか等しい場合true()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Man2 >= TileKind.Man1);
        Assert.True(TileKind.Man5 >= TileKind.Man1);
        Assert.True(TileKind.Man9 >= TileKind.Man5);
        Assert.True(TileKind.Man1 >= TileKind.Man1);
        Assert.False(TileKind.Man1 >= TileKind.Man2);
        Assert.False(TileKind.Man2 >= TileKind.Man5);
    }

    [Fact]
    public void 小なり_nullを含む比較_nullは最小として扱われる()
    {
        // Arrange & Act & Assert
        Assert.True((TileKind?)null < TileKind.Man1);
        Assert.False(TileKind.Man1 < (TileKind?)null);
        Assert.False((TileKind?)null < (TileKind?)null);
    }

    [Fact]
    public void 小なりイコール_nullを含む比較_nullは最小として扱われる()
    {
        // Arrange & Act & Assert
        Assert.True((TileKind?)null <= TileKind.Man1);
        Assert.False(TileKind.Man1 <= (TileKind?)null);
        Assert.True((TileKind?)null <= (TileKind?)null);
    }

    [Fact]
    public void 大なり_nullを含む比較_nullは最小として扱われる()
    {
        // Arrange & Act & Assert
        Assert.False((TileKind?)null > TileKind.Man1);
        Assert.True(TileKind.Man1 > (TileKind?)null);
        Assert.False((TileKind?)null > (TileKind?)null);
    }

    [Fact]
    public void 大なりイコール_nullを含む比較_nullは最小として扱われる()
    {
        // Arrange & Act & Assert
        Assert.False((TileKind?)null >= TileKind.Man1);
        Assert.True(TileKind.Man1 >= (TileKind?)null);
        Assert.True((TileKind?)null >= (TileKind?)null);
    }
}
