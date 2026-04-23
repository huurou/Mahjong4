using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Shantens;

public class ShantenCalculator_CalcTests
{
    [Theory]
    [InlineData("567", "11", "111234567", "", ShantenConstants.SHANTEN_AGARI)]
    [InlineData("567", "11", "111345677", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("567", "15", "111345677", "", 1)]
    [InlineData("1578", "15", "11134567", "", 2)]
    [InlineData("1358", "1358", "113456", "", 3)]
    [InlineData("1358", "13588", "1589", "t", 4)]
    [InlineData("1358", "13588", "159", "tn", 5)]
    [InlineData("1358", "258", "1589", "tns", 6)]
    [InlineData("", "", "11123456788999", "", ShantenConstants.SHANTEN_AGARI)]
    [InlineData("", "", "11122245679999", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("8", "1367", "4566677", "tn", 2)]
    [InlineData("3678", "3356", "15", "nhrc", 4)]
    [InlineData("359", "17", "159", "tnshrc", 7)]
    [InlineData("1111222235555", "", "", "t", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("1358", "13588", "589", "tt", 3)]
    [InlineData("1358", "13588", "59", "ttt", 3)]
    [InlineData("1358", "1388", "59", "tttt", 3)]
    [InlineData("", "11", "345677788899", "", ShantenConstants.SHANTEN_AGARI)]
    public void 通常形14枚_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("567", "1", "111345677", "", 1)]
    [InlineData("567", "", "111345677", "", 1)]
    [InlineData("56", "", "111345677", "", 0)]
    public void 通常形13枚_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("77", "114477", "114477", "", ShantenConstants.SHANTEN_AGARI)]
    [InlineData("76", "114477", "114477", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("76", "114479", "114477", "", 1)]
    [InlineData("76", "14479", "114477", "t", 2)]
    [InlineData("76", "13479", "114477", "t", 3)]
    [InlineData("76", "13479", "114467", "t", 4)]
    [InlineData("76", "13479", "113467", "t", 5)]
    [InlineData("76", "13479", "123467", "t", 6)]
    public void 七対子_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: false, useChiitoitsu: true, useKokushi: false);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("19", "19", "19", "tnsphrcc", ShantenConstants.SHANTEN_AGARI)]
    [InlineData("19", "19", "129", "tnsphrc", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("19", "129", "129", "tnsphr", 1)]
    [InlineData("129", "129", "129", "tnsph", 2)]
    [InlineData("129", "129", "1239", "nsph", 3)]
    [InlineData("129", "1239", "1239", "sph", 4)]
    [InlineData("1239", "1239", "1239", "ph", 5)]
    [InlineData("1239", "1239", "12349", "h", 6)]
    [InlineData("1239", "12349", "12349", "", 7)]
    public void 国士無双_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: false, useChiitoitsu: false, useKokushi: true);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123456789", "", "", "tttt", 1)]
    [InlineData("123456789", "1111", "", "", 1)]
    [InlineData("77779", "12345", "1234", "", 1)]
    public void 手牌に同種の牌が4枚ある_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 数牌4枚槓子と3面子のみ_対子がないので1シャンテン扱いになる()
    {
        // Arrange: 1111m + 234p + 567p + 123s = 13 枚。面子 4 個 (1m 槓子 + 3 面子) だが対子なし。
        // ナイーブには「1m 槓子を 111m + 孤立 1m として 4 面子 + 単騎待ち」と見えるため shanten=0 (テンパイ) に
        // なりそうだが、槓子を部分的に使った単騎待ちは実際には成立しない (同牌が 4 枚全部使われている)。
        // ShantenCalculator.UpdateBestShanten の isolatedOnlyFromNumberKantsu 経路が +1 補正する対象ケース。
        var hand = new TileKindList("1111", "234567", "123", "");

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert: 1 シャンテン (対子を作る 1 枚が必要)
        Assert.Equal(1, actual);
    }

    [Theory]
    [InlineData("", "222567", "44468", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("", "222567", "68", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("", "", "68", "", ShantenConstants.SHANTEN_TENPAI)]
    [InlineData("", "", "88", "", ShantenConstants.SHANTEN_AGARI)]
    public void 手牌が13枚より少ない_正しいシャンテン数を取得できる(string man, string pin, string sou, string honor, int expected)
    {
        // Arrange
        var hand = new TileKindList(man, pin, sou, honor);

        // Act
        var actual = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 手牌が14枚を超える_例外がスローされる()
    {
        // Arrange
        var hand = new TileKindList("123456789", "123456", "", "");

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand));

        // Assert
        var argEx = Assert.IsType<ArgumentException>(exception);
        Assert.Contains("手牌の数が14個より多いです", argEx.Message);
        Assert.Equal("tileKindList", argEx.ParamName);
    }

    [Fact]
    public void 全ての形が無効_例外がスローされる()
    {
        // Arrange
        var hand = new TileKindList("123456789", "1234", "", "");

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand, useRegular: false, useChiitoitsu: false, useKokushi: false));

        // Assert
        var argEx = Assert.IsType<ArgumentException>(exception);
        Assert.Contains("最低でも1つの形を指定してください", argEx.Message);
    }

    [Fact]
    public void 通常形_同種牌が5枚以上_例外がスローされる()
    {
        // Arrange - Man1が5枚（不正な手牌だがCount<=14のためCount判定は通過）
        var hand = new TileKindList("111112345", "12345", "", "");

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false));

        // Assert
        var argEx = Assert.IsType<ArgumentException>(exception);
        Assert.Contains("同じ牌種が5枚以上含まれています", argEx.Message);
        Assert.Equal("tileKindList", argEx.ParamName);
    }
}
