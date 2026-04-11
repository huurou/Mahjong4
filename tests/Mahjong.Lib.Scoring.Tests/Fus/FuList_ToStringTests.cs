using Mahjong.Lib.Scoring.Fus;

namespace Mahjong.Lib.Scoring.Tests.Fus;

public class FuList_ToStringTests
{
    [Fact]
    public void 空のFuListの場合_0符を返す()
    {
        // Arrange
        var fuList = new FuList();

        // Act & Assert
        Assert.Equal("0符 ", fuList.ToString());
    }

    [Fact]
    public void 単一の符の場合_正しい文字列表現を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei]);

        // Act & Assert
        Assert.Equal("20符 副底:20符", fuList.ToString());
    }

    [Fact]
    public void 複数の符の場合_正しい文字列表現を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen, Fu.Tsumo]);

        // Act & Assert
        Assert.Equal("40符 副底:20符,面前加符:10符,ツモ符:2符", fuList.ToString());
    }

    [Fact]
    public void 七対子符の場合_25符を返す()
    {
        // Arrange
        var fuList = new FuList([Fu.Chiitoitsu]);

        // Act & Assert
        Assert.Equal("25符 七対子符:25符", fuList.ToString());
    }
}
