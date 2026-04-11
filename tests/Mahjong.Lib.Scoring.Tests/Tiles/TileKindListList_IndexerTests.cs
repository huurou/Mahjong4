using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_IndexerTests
{
    [Fact]
    public void 有効なインデックス_正しいTileKindListを返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789"),
        ]);

        // Act & Assert
        Assert.Equal(new TileKindList(man: "123"), list[0]);
        Assert.Equal(new TileKindList(pin: "456"), list[1]);
        Assert.Equal(new TileKindList(sou: "789"), list[2]);
    }

    [Fact]
    public void 負のインデックスを指定_ArgumentOutOfRangeExceptionが発生する()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);

        // Act
        var ex = Record.Exception(() => list[-1]);

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact]
    public void 要素数以上のインデックスを指定_ArgumentOutOfRangeExceptionが発生する()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);

        // Act
        var ex = Record.Exception(() => list[1]);

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }
}
