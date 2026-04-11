using Mahjong.Lib.Scoring.Fus;

namespace Mahjong.Lib.Scoring.Tests.Fus;

public class FuList_GetHashCodeTests
{
    [Fact]
    public void 同じ符を含むFuList_同じハッシュコードを返す()
    {
        // Arrange
        var fuList1 = new FuList([Fu.Futei, Fu.Menzen]);
        var fuList2 = new FuList([Fu.Futei, Fu.Menzen]);

        // Act & Assert
        Assert.Equal(fuList1.GetHashCode(), fuList2.GetHashCode());
    }

    [Fact]
    public void 異なる符を含むFuList_異なるハッシュコードを返す可能性が高い()
    {
        // Arrange
        var fuList1 = new FuList([Fu.Futei]);
        var fuList2 = new FuList([Fu.Menzen]);

        // Act & Assert
        Assert.NotEqual(fuList1.GetHashCode(), fuList2.GetHashCode());
    }
}
