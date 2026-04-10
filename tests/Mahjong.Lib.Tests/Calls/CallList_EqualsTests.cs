using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class CallList_EqualsTests
{
    [Fact]
    public void 同じ副露構成で別インスタンス_等しい()
    {
        // Arrange
        var callList1 = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        ]);
        var callList2 = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        ]);

        // Act & Assert
        Assert.Equal(callList1, callList2);
        Assert.Equal(callList1.GetHashCode(), callList2.GetHashCode());
    }

    [Fact]
    public void 異なる副露構成_等しくない()
    {
        // Arrange
        var callList1 = new CallList([Call.Chi(new TileKindList(man: "123"))]);
        var callList2 = new CallList([Call.Pon(new TileKindList(man: "111"))]);

        // Act & Assert
        Assert.NotEqual(callList1, callList2);
    }

    [Fact]
    public void 同じインスタンス_等しい()
    {
        // Arrange
        var callList = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act & Assert
        Assert.Equal(callList, callList);
    }

    [Fact]
    public void Nullとの比較_等しくない()
    {
        // Arrange
        var callList = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act & Assert
        Assert.False(callList.Equals(null));
    }

    [Fact]
    public void 空のCallList同士_等しい()
    {
        // Arrange
        var callList1 = new CallList();
        var callList2 = new CallList();

        // Act & Assert
        Assert.Equal(callList1, callList2);
        Assert.Equal(callList1.GetHashCode(), callList2.GetHashCode());
    }

    [Fact]
    public void 順序が異なる副露_ソートされるため等しい()
    {
        // Arrange
        var callList1 = new CallList([
            Call.Pon(new TileKindList(man: "111")),
            Call.Chi(new TileKindList(man: "123")),
        ]);
        var callList2 = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        ]);

        // Act & Assert
        Assert.Equal(callList1, callList2);
        Assert.Equal(callList1.GetHashCode(), callList2.GetHashCode());
    }
}
