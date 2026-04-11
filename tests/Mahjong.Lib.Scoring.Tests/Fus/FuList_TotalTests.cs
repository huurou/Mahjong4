using Mahjong.Lib.Scoring.Fus;

namespace Mahjong.Lib.Scoring.Tests.Fus;

public class FuList_TotalTests
{
    [Fact]
    public void 空のFuListの場合_0を返す()
    {
        // Arrange
        var fuList = new FuList();

        // Act & Assert
        Assert.Equal(0, fuList.Total);
    }

    [Fact]
    public void 七対子符が含まれる場合_25を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Chiitoitsu]);

        // Act & Assert
        Assert.Equal(25, fuList.Total);
    }

    [Fact]
    public void 七対子符と他の符が含まれる場合_25を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Chiitoitsu, Fu.Tsumo]);

        // Act & Assert
        Assert.Equal(25, fuList.Total); // 七対子の場合は他の符に関係なく25符固定
    }

    [Fact]
    public void 通常の符の場合_10の単位に切り上げられる()
    {
        // Arrange & Act & Assert

        // 22符 → 30符
        var fuList22 = new FuList([Fu.Futei, Fu.Tsumo]); // 20 + 2 = 22
        Assert.Equal(30, fuList22.Total);

        // 30符 → 30符
        var fuList30 = new FuList([Fu.FuteiOpenPinfu]); // 30
        Assert.Equal(30, fuList30.Total);

        // 32符 → 40符
        var fuList32 = new FuList([Fu.Futei, Fu.Menzen, Fu.Tsumo]); // 20 + 10 + 2 = 32
        Assert.Equal(40, fuList32.Total);
    }
}
