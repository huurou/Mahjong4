using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_IncludesKoutsuTests
{
    [Fact]
    public void 刻子が含まれている場合_Trueを返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "111"), // 刻子
            new TileKindList(pin: "123"),  // 順子
        ]);

        // Act & Assert
        Assert.True(list.IncludesKoutsu(TileKind.Man1));
        Assert.False(list.IncludesKoutsu(TileKind.Pin1));
    }

    [Fact]
    public void 槓子が含まれている場合_Trueを返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "1111"), // 槓子
            new TileKindList(pin: "123"),   // 順子
        ]);

        // Act & Assert
        Assert.True(list.IncludesKoutsu(TileKind.Man1));
        Assert.False(list.IncludesKoutsu(TileKind.Pin1));
    }

    [Fact]
    public void 刻子も槓子も含まれていない場合_Falseを返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "123"), // 順子
            new TileKindList(pin: "456"),  // 順子
        ]);

        // Act & Assert
        Assert.False(list.IncludesKoutsu(TileKind.Man1));
        Assert.False(list.IncludesKoutsu(TileKind.Pin4));
    }

    [Fact]
    public void 空のリスト_Falseを返す()
    {
        // Arrange
        var list = new TileKindListList();

        // Act & Assert
        Assert.False(list.IncludesKoutsu(TileKind.Man1));
    }
}
