using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_EqualsTests
{
    [Fact]
    public void 同じFuType_等しいと判定される()
    {
        // Arrange
        var fu1 = Fu.Tsumo;
        var fu2 = Fu.Tsumo;

        // Act & Assert
        Assert.Equal(fu1, fu2);
        Assert.True(fu1.Equals(fu2));
        Assert.True(fu1 == fu2);
        Assert.False(fu1 != fu2);
        Assert.Equal(fu1.GetHashCode(), fu2.GetHashCode());
    }

    [Fact]
    public void 異なるFuType_等しくないと判定される()
    {
        // Arrange
        var fu1 = Fu.Tsumo;
        var fu2 = Fu.Kanchan;

        // Act & Assert
        Assert.NotEqual(fu1, fu2);
        Assert.False(fu1.Equals(fu2));
        Assert.False(fu1 == fu2);
        Assert.True(fu1 != fu2);
    }

    [Fact]
    public void Null_等しくないと判定される()
    {
        // Arrange
        var fu = Fu.Tsumo;

        // Act & Assert
        Assert.NotNull(fu);
        Assert.False(fu.Equals(null));
    }

    [Fact]
    public void 異なる型_等しくないと判定される()
    {
        // Arrange
        var fu = Fu.Tsumo;
        var obj = new object();

        // Act & Assert
        Assert.NotEqual(fu, obj);
        Assert.False(fu.Equals(obj));
    }
}
