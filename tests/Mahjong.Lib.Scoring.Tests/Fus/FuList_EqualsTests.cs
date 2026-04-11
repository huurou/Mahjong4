using Mahjong.Lib.Scoring.Fus;

namespace Mahjong.Lib.Scoring.Tests.Fus;

public class FuList_EqualsTests
{
    [Fact]
    public void 同じ符を含むFuList_等しいと判定される()
    {
        // Arrange
        var fuList1 = new FuList([Fu.Futei, Fu.Menzen]);
        var fuList2 = new FuList([Fu.Futei, Fu.Menzen]);

        // Act & Assert
        Assert.Equal(fuList1, fuList2);
        Assert.True(fuList1.Equals(fuList2));
        Assert.True(fuList1 == fuList2);
        Assert.False(fuList1 != fuList2);
    }

    [Fact]
    public void 異なる符を含むFuList_等しくないと判定される()
    {
        // Arrange
        var fuList1 = new FuList([Fu.Futei]);
        var fuList2 = new FuList([Fu.Menzen]);

        // Act & Assert
        Assert.NotEqual(fuList1, fuList2);
        Assert.False(fuList1.Equals(fuList2));
        Assert.False(fuList1 == fuList2);
        Assert.True(fuList1 != fuList2);
    }

    [Fact]
    public void 順序が異なる同じ符のFuList_等しいと判定される()
    {
        // Arrange
        var fuList1 = new FuList([Fu.Futei, Fu.Menzen]);
        var fuList2 = new FuList([Fu.Menzen, Fu.Futei]);

        // Act & Assert
        Assert.Equal(fuList1, fuList2);
        Assert.True(fuList1.Equals(fuList2));
        Assert.Equal(fuList1.GetHashCode(), fuList2.GetHashCode());
    }

    [Fact]
    public void Null_等しくないと判定される()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei]);

        // Act & Assert
        Assert.NotNull(fuList);
        Assert.False(fuList.Equals(null));
    }

    [Fact]
    public void 同一参照_等しいと判定される()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen]);

        // Act
        var result = fuList.Equals(fuList);

        // Assert
        Assert.True(result);
    }
}
