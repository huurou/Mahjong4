using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class CallList_CompareToTests
{
    [Fact]
    public void 同じ副露構成_0を返す()
    {
        // Arrange
        var callList1 = new CallList([Call.Chi(new TileKindList(man: "123"))]);
        var callList2 = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act
        var result = callList1.CompareTo(callList2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void 異なる副露構成_正しい順序で比較される()
    {
        // Arrange
        var callList1 = new CallList([Call.Chi(new TileKindList(man: "123"))]);
        var callList2 = new CallList([Call.Pon(new TileKindList(man: "111"))]);

        // Act & Assert
        Assert.True(callList1.CompareTo(callList2) < 0);
        Assert.True(callList2.CompareTo(callList1) > 0);
    }

    [Fact]
    public void 要素数が異なる場合_少ない方が前になる()
    {
        // Arrange
        var callList1 = new CallList([Call.Chi(new TileKindList(man: "123"))]);
        var callList2 = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        ]);

        // Act & Assert
        Assert.True(callList1.CompareTo(callList2) < 0);
        Assert.True(callList2.CompareTo(callList1) > 0);
    }

    [Fact]
    public void Nullと比較_正の値を返す()
    {
        // Arrange
        var callList = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act
        var result = callList.CompareTo(null);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void 比較演算子_正しく動作する()
    {
        // Arrange
        var callList1 = new CallList([Call.Chi(new TileKindList(man: "123"))]);
        var callList2 = new CallList([Call.Pon(new TileKindList(man: "111"))]);
        var callList3 = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act & Assert
        Assert.True(callList1 < callList2);
        Assert.True(callList2 > callList1);
        Assert.True(callList1 <= callList3);
        Assert.True(callList1 >= callList3);
        Assert.False(callList1 > callList2);
        Assert.False(callList2 < callList1);
    }
}
