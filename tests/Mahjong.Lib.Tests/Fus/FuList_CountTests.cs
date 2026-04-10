using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class FuList_CountTests
{
    [Fact]
    public void 空のFuListの場合_0を返す()
    {
        // Arrange
        var fuList = new FuList();

        // Act & Assert
        Assert.Equal(0, fuList.Count);
    }

    [Fact]
    public void 符が含まれる場合_符の数を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen]);

        // Act & Assert
        Assert.Equal(2, fuList.Count);
    }
}
