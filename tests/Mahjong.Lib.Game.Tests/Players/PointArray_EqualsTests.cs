using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class PointArray_EqualsTests
{
    [Fact]
    public void 同じ初期値の別インスタンス_等価になる()
    {
        // Arrange
        var a = new PointArray(new Point(25000));
        var b = new PointArray(new Point(25000));

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なる初期値_非等価になる()
    {
        // Arrange
        var a = new PointArray(new Point(25000));
        var b = new PointArray(new Point(30000));

        // Act & Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void 同じ操作後の別インスタンス_等価になる()
    {
        // Arrange
        var a = new PointArray(new Point(25000)).AddPoint(new PlayerIndex(0), 1000);
        var b = new PointArray(new Point(25000)).AddPoint(new PlayerIndex(0), 1000);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
