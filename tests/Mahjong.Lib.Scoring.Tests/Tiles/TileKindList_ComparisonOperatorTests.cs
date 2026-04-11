using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_ComparisonOperatorTests
{
    [Fact]
    public void 小なり演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.True(new TileKindList(man: "122") < new TileKindList(man: "123"));
        Assert.False(new TileKindList(man: "123") < new TileKindList(man: "122"));
        Assert.False(new TileKindList(man: "122") < new TileKindList(man: "122"));
    }

    [Fact]
    public void 大なり演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.False(new TileKindList(man: "122") > new TileKindList(man: "123"));
        Assert.True(new TileKindList(man: "123") > new TileKindList(man: "122"));
        Assert.False(new TileKindList(man: "122") > new TileKindList(man: "122"));
    }

    [Fact]
    public void 以下演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.True(new TileKindList(man: "122") <= new TileKindList(man: "123"));
        Assert.False(new TileKindList(man: "123") <= new TileKindList(man: "122"));
        Assert.True(new TileKindList(man: "122") <= new TileKindList(man: "122"));
    }

    [Fact]
    public void 以上演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.False(new TileKindList(man: "122") >= new TileKindList(man: "123"));
        Assert.True(new TileKindList(man: "123") >= new TileKindList(man: "122"));
        Assert.True(new TileKindList(man: "122") >= new TileKindList(man: "122"));
    }

    [Fact]
    public void 異なる長さのリスト_正しい結果を返す()
    {
        // Act & Assert
        Assert.True(new TileKindList(man: "12") < new TileKindList(man: "123"));
        Assert.False(new TileKindList(man: "12") > new TileKindList(man: "123"));
        Assert.True(new TileKindList(man: "12") <= new TileKindList(man: "123"));
        Assert.False(new TileKindList(man: "12") >= new TileKindList(man: "123"));
        Assert.False(new TileKindList(man: "12") == new TileKindList(man: "123"));
        Assert.True(new TileKindList(man: "12") != new TileKindList(man: "123"));
    }

    [Fact]
    public void Nullとの比較演算_正しい結果を返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");
        TileKindList? nullList = null;

        // Act & Assert
        Assert.False(list < nullList);
        Assert.True(list > nullList);
        Assert.False(list <= nullList);
        Assert.True(list >= nullList);
    }

    [Fact]
    public void 左辺がnull_正しい結果を返す()
    {
        // Arrange
        TileKindList? nullList = null;
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(nullList < list);
        Assert.False(nullList > list);
        Assert.True(nullList <= list);
        Assert.False(nullList >= list);
    }

    [Fact]
    public void 両辺がnull_正しい結果を返す()
    {
        // Arrange
        TileKindList? null1 = null;
        TileKindList? null2 = null;

        // Act & Assert
        Assert.False(null1 < null2);
        Assert.False(null1 > null2);
        Assert.True(null1 <= null2);
        Assert.True(null1 >= null2);
    }
}
