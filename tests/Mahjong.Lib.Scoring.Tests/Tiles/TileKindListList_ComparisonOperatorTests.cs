using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_ComparisonOperatorTests
{
    [Fact]
    public void 小なり演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.True(new TileKindListList([new TileKindList(man: "122")]) < new TileKindListList([new TileKindList(man: "123")]));
        Assert.False(new TileKindListList([new TileKindList(man: "123")]) < new TileKindListList([new TileKindList(man: "122")]));
        Assert.False(new TileKindListList([new TileKindList(man: "122")]) < new TileKindListList([new TileKindList(man: "122")]));
    }

    [Fact]
    public void 大なり演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.False(new TileKindListList([new TileKindList(man: "122")]) > new TileKindListList([new TileKindList(man: "123")]));
        Assert.True(new TileKindListList([new TileKindList(man: "123")]) > new TileKindListList([new TileKindList(man: "122")]));
        Assert.False(new TileKindListList([new TileKindList(man: "122")]) > new TileKindListList([new TileKindList(man: "122")]));
    }

    [Fact]
    public void 以下演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.True(new TileKindListList([new TileKindList(man: "122")]) <= new TileKindListList([new TileKindList(man: "123")]));
        Assert.False(new TileKindListList([new TileKindList(man: "123")]) <= new TileKindListList([new TileKindList(man: "122")]));
        Assert.True(new TileKindListList([new TileKindList(man: "122")]) <= new TileKindListList([new TileKindList(man: "122")]));
    }

    [Fact]
    public void 以上演算子_正しい結果を返す()
    {
        // Act & Assert
        Assert.False(new TileKindListList([new TileKindList(man: "122")]) >= new TileKindListList([new TileKindList(man: "123")]));
        Assert.True(new TileKindListList([new TileKindList(man: "123")]) >= new TileKindListList([new TileKindList(man: "122")]));
        Assert.True(new TileKindListList([new TileKindList(man: "122")]) >= new TileKindListList([new TileKindList(man: "122")]));
    }

    [Fact]
    public void Nullとの比較演算_正しい結果を返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);
        TileKindListList? nullList = null;

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
        TileKindListList? nullList = null;
        var list = new TileKindListList([new TileKindList(man: "123")]);

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
        TileKindListList? null1 = null;
        TileKindListList? null2 = null;

        // Act & Assert
        Assert.False(null1 < null2);
        Assert.False(null1 > null2);
        Assert.True(null1 <= null2);
        Assert.True(null1 >= null2);
    }

    [Fact]
    public void 等値演算子_同じ内容_Trueを返す()
    {
        // Arrange
        var list1 = new TileKindListList([new TileKindList(man: "123")]);
        var list2 = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.True(list1 == list2);
        Assert.False(list1 != list2);
    }

    [Fact]
    public void 等値演算子_異なる内容_Falseを返す()
    {
        // Arrange
        var list1 = new TileKindListList([new TileKindList(man: "123")]);
        var list2 = new TileKindListList([new TileKindList(man: "456")]);

        // Act & Assert
        Assert.False(list1 == list2);
        Assert.True(list1 != list2);
    }

    [Fact]
    public void 等値演算子_nullとの比較_正しい結果を返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);
        TileKindListList? nullList = null;

        // Act & Assert
        Assert.False(list == nullList);
        Assert.False(nullList == list);
        Assert.True(list != nullList);
        Assert.True(nullList != list);
    }
}
