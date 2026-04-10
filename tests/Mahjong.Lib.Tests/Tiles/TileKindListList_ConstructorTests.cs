using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空のリスト_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindListList();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Empty(list);
    }

    [Fact]
    public void IEnumerableコンストラクタ_TileKindList配列指定_正常に作成される()
    {
        // Arrange
        var tileKindLists = new[]
        {
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789"),
        };

        // Act
        var list = new TileKindListList(tileKindLists);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(new TileKindList(man: "123"), list[0]);
        Assert.Equal(new TileKindList(pin: "456"), list[1]);
        Assert.Equal(new TileKindList(sou: "789"), list[2]);
    }

    [Fact]
    public void コレクション初期化構文_TileKindList配列指定_正常に作成される()
    {
        // Arrange & Act
        TileKindListList list = [
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789"),
        ];

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(new TileKindList(man: "123"), list[0]);
        Assert.Equal(new TileKindList(pin: "456"), list[1]);
        Assert.Equal(new TileKindList(sou: "789"), list[2]);
    }

    [Fact]
    public void Count_要素数を取得_正しい値が返される()
    {
        // Arrange
        var emptyList = new TileKindListList();
        var listWithThreeItems = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789"),
        ]);

        // Act & Assert
        Assert.Equal(0, emptyList.Count);
        Assert.Equal(3, listWithThreeItems.Count);
    }
}
