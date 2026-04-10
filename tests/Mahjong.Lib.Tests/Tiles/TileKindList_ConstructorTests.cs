using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空のリスト_Countが0()
    {
        // Arrange & Act
        var list = new TileKindList();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Empty(list);
    }

    [Fact]
    public void IEnumerableコンストラクタ_牌種別配列指定_ソート済みで作成される()
    {
        // Arrange
        var tiles = new[] { TileKind.Man3, TileKind.Man1, TileKind.Man2 };

        // Act
        var list = new TileKindList(tiles);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(TileKind.Man1, list[0]);
        Assert.Equal(TileKind.Man2, list[1]);
        Assert.Equal(TileKind.Man3, list[2]);
    }

    [Fact]
    public void 文字列コンストラクタ_萬子_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindList(man: "123");

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(TileKind.Man1, list[0]);
        Assert.Equal(TileKind.Man2, list[1]);
        Assert.Equal(TileKind.Man3, list[2]);
    }

    [Fact]
    public void 文字列コンストラクタ_筒子_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindList(pin: "456");

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(TileKind.Pin4, list[0]);
        Assert.Equal(TileKind.Pin5, list[1]);
        Assert.Equal(TileKind.Pin6, list[2]);
    }

    [Fact]
    public void 文字列コンストラクタ_索子_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindList(sou: "789");

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(TileKind.Sou7, list[0]);
        Assert.Equal(TileKind.Sou8, list[1]);
        Assert.Equal(TileKind.Sou9, list[2]);
    }

    [Fact]
    public void 文字列コンストラクタ_字牌アルファベット_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindList(honor: "tnsphrc");

        // Assert
        Assert.Equal(7, list.Count);
        Assert.Equal(TileKind.Ton, list[0]);
        Assert.Equal(TileKind.Nan, list[1]);
        Assert.Equal(TileKind.Sha, list[2]);
        Assert.Equal(TileKind.Pei, list[3]);
        Assert.Equal(TileKind.Haku, list[4]);
        Assert.Equal(TileKind.Hatsu, list[5]);
        Assert.Equal(TileKind.Chun, list[6]);
    }

    [Fact]
    public void 文字列コンストラクタ_字牌日本語_正常に作成される()
    {
        // Arrange & Act
        var list = new TileKindList(honor: "東南西北白發中");

        // Assert
        Assert.Equal(7, list.Count);
        Assert.Equal(TileKind.Ton, list[0]);
        Assert.Equal(TileKind.Nan, list[1]);
        Assert.Equal(TileKind.Sha, list[2]);
        Assert.Equal(TileKind.Pei, list[3]);
        Assert.Equal(TileKind.Haku, list[4]);
        Assert.Equal(TileKind.Hatsu, list[5]);
        Assert.Equal(TileKind.Chun, list[6]);
    }

    [Fact]
    public void 文字列コンストラクタ_複合指定_ソート済みで作成される()
    {
        // Arrange & Act
        var list = new TileKindList(man: "123", pin: "456", sou: "789", honor: "t");

        // Assert
        Assert.Equal(10, list.Count);
        // 萬子
        Assert.Equal(TileKind.Man1, list[0]);
        Assert.Equal(TileKind.Man2, list[1]);
        Assert.Equal(TileKind.Man3, list[2]);
        // 筒子
        Assert.Equal(TileKind.Pin4, list[3]);
        Assert.Equal(TileKind.Pin5, list[4]);
        Assert.Equal(TileKind.Pin6, list[5]);
        // 索子
        Assert.Equal(TileKind.Sou7, list[6]);
        Assert.Equal(TileKind.Sou8, list[7]);
        Assert.Equal(TileKind.Sou9, list[8]);
        // 字牌
        Assert.Equal(TileKind.Ton, list[9]);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な萬子範囲_ArgumentOutOfRangeException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(man: "0"));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("man", ((ArgumentOutOfRangeException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な萬子文字_ArgumentException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(man: "a"));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("man", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な筒子文字_ArgumentException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(pin: "abc"));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("pin", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な筒子範囲_ArgumentOutOfRangeException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(pin: "0"));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("pin", ((ArgumentOutOfRangeException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な索子範囲_ArgumentOutOfRangeException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(sou: "0"));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("sou", ((ArgumentOutOfRangeException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な索子文字_ArgumentException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(sou: "a"));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("sou", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 文字列コンストラクタ_無効な字牌ASCII_ArgumentException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(honor: "x"));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("honor", ((ArgumentException)ex).ParamName);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("i")]
    [InlineData("o")]
    [InlineData("q")]
    [InlineData("あ")]
    [InlineData("双")]
    [InlineData("年")]
    [InlineData("海")]
    [InlineData("花")]
    [InlineData("零")]
    public void 文字列コンストラクタ_無効な字牌_ArgumentException発生_各種文字(string honor)
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKindList(honor: honor));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("honor", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void コレクション式_牌種別が正しく作成される()
    {
        // Arrange & Act
        TileKindList list = [TileKind.Man1, TileKind.Man2, TileKind.Man3];

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(TileKind.Man1, list[0]);
        Assert.Equal(TileKind.Man2, list[1]);
        Assert.Equal(TileKind.Man3, list[2]);
    }
}
