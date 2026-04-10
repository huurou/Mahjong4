using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_CompareToTests
{
    [Fact]
    public void Numberが小さい方_負の値を返す()
    {
        // Arrange
        var fu1 = Fu.Futei;    // Number: 0
        var fu2 = Fu.Menzen;   // Number: 1

        // Act
        var result = fu1.CompareTo(fu2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void Numberが大きい方_正の値を返す()
    {
        // Arrange
        var fu1 = Fu.Tsumo;    // Number: 4
        var fu2 = Fu.Menzen;   // Number: 1

        // Act
        var result = fu1.CompareTo(fu2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void 同じFu_0を返す()
    {
        // Arrange
        var fu1 = Fu.Tsumo;
        var fu2 = Fu.Tsumo;

        // Act
        var result = fu1.CompareTo(fu2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void null_正の値を返す()
    {
        // Arrange
        var fu = Fu.Tsumo;

        // Act
        var result = fu.CompareTo(null);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void 同じNumberで異なるType_Typeで比較される()
    {
        // Arrange
        var fu1 = new Fu(FuType.Futei, 1);
        var fu2 = new Fu(FuType.Menzen, 1);

        // Act
        var result = fu1.CompareTo(fu2);

        // Assert
        Assert.True(result < 0); // FuType.Futei(0) < FuType.Menzen(1)
    }
}
