using Mahjong.Lib.Scoring.Fus;

namespace Mahjong.Lib.Scoring.Tests.Fus;

public class FuList_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空のFuListが作成される()
    {
        // Arrange & Act
        var fuList = new FuList();

        // Assert
        Assert.Equal(0, fuList.Count);
        Assert.Equal(0, fuList.Total);
        Assert.Empty(fuList);
    }

    [Fact]
    public void コレクションコンストラクタ_符のコレクションから作成される()
    {
        // Arrange
        var fus = new[] { Fu.Futei, Fu.Menzen, Fu.Tsumo };

        // Act
        var fuList = new FuList(fus);

        // Assert
        Assert.Equal(3, fuList.Count);
        Assert.Equal(40, fuList.Total); // (20 + 10 + 2 + 9) / 10 * 10 = 40
        Assert.Contains(Fu.Futei, fuList);
        Assert.Contains(Fu.Menzen, fuList);
        Assert.Contains(Fu.Tsumo, fuList);
    }

    [Fact]
    public void コレクションコンストラクタ_空のコレクションから作成される()
    {
        // Arrange
        var fus = Array.Empty<Fu>();

        // Act
        var fuList = new FuList(fus);

        // Assert
        Assert.Equal(0, fuList.Count);
        Assert.Equal(0, fuList.Total);
        Assert.Empty(fuList);
    }

    [Fact]
    public void コレクションコンストラクタ_順不同の入力でもソートされる()
    {
        // Arrange
        var fus = new[] { Fu.Tsumo, Fu.Futei, Fu.Menzen }; // Type ordinal: 4, 0, 1

        // Act
        var fuList = new FuList(fus);

        // Assert
        var items = fuList.ToList();
        Assert.Equal(Fu.Futei, items[0]);   // Type ordinal: 0
        Assert.Equal(Fu.Menzen, items[1]);   // Type ordinal: 1
        Assert.Equal(Fu.Tsumo, items[2]);    // Type ordinal: 4
    }

    [Fact]
    public void コレクションビルダー_コレクション式で作成できる()
    {
        // Arrange & Act
        FuList fuList = [Fu.Futei, Fu.Menzen, Fu.Tsumo];

        // Assert
        Assert.Equal(3, fuList.Count);
        Assert.Contains(Fu.Futei, fuList);
        Assert.Contains(Fu.Menzen, fuList);
        Assert.Contains(Fu.Tsumo, fuList);
        Assert.Equal("40符 副底:20符,面前加符:10符,ツモ符:2符", fuList.ToString());
    }
}
