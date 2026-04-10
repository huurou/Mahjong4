using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_CompareToTests
{
    [Fact]
    public void 比較_Value順で正しく比較される()
    {
        // Arrange
        var tile1 = TileKind.Man1;
        var tile5 = TileKind.Man5;
        var tile9 = TileKind.Man9;

        // Act & Assert
        // 同一値
        Assert.Equal(0, tile1.CompareTo(TileKind.Man1));
        Assert.Equal(0, tile5.CompareTo(TileKind.Man5));

        // 小さい値との比較（正の値）
        Assert.True(tile5.CompareTo(tile1) > 0);
        Assert.True(tile9.CompareTo(tile5) > 0);

        // 大きい値との比較（負の値）
        Assert.True(tile1.CompareTo(tile5) < 0);
        Assert.True(tile5.CompareTo(tile9) < 0);

        // null比較
        Assert.Equal(1, tile1.CompareTo(null));
    }
}
