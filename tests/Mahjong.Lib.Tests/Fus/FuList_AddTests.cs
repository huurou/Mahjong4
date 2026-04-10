using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class FuList_AddTests
{
    [Fact]
    public void 符が追加された新しいFuListが返される()
    {
        // Arrange
        var originalFuList = new FuList([Fu.Futei]);

        // Act
        var newFuList = originalFuList.Add(Fu.Menzen);

        // Assert
        Assert.Equal(1, originalFuList.Count);
        Assert.Equal(2, newFuList.Count);
        Assert.Contains(Fu.Futei, newFuList);
        Assert.Contains(Fu.Menzen, newFuList);
    }

    [Fact]
    public void 空のFuListに符を追加()
    {
        // Arrange
        var originalFuList = new FuList();

        // Act
        var newFuList = originalFuList.Add(Fu.Tsumo);

        // Assert
        Assert.Equal(0, originalFuList.Count);
        Assert.Equal(1, newFuList.Count);
        Assert.Contains(Fu.Tsumo, newFuList);
    }

    [Fact]
    public void 不変性の確認_Addしても元のインスタンスは変更されない()
    {
        // Arrange
        var originalFus = new[] { Fu.Futei };
        var originalFuList = new FuList(originalFus);
        var originalCount = originalFuList.Count;
        var originalTotal = originalFuList.Total;

        // Act
        var newFuList = originalFuList.Add(Fu.Menzen);

        // Assert
        Assert.Equal(originalCount, originalFuList.Count);
        Assert.Equal(originalTotal, originalFuList.Total);
        Assert.NotEqual(originalFuList.Count, newFuList.Count);
        Assert.NotSame(originalFuList, newFuList);
    }
}
