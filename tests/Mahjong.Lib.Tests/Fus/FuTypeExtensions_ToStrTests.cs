using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class FuTypeExtensions_ToStrTests
{
    [Theory]
    [InlineData(FuType.Futei, "副底")]
    [InlineData(FuType.Menzen, "面前加符")]
    [InlineData(FuType.Chiitoitsu, "七対子符")]
    [InlineData(FuType.FuteiOpenPinfu, "副底(食い平和)")]
    [InlineData(FuType.Tsumo, "ツモ符")]
    [InlineData(FuType.Kanchan, "カンチャン待ち")]
    [InlineData(FuType.Penchan, "ペンチャン待ち")]
    [InlineData(FuType.Tanki, "単騎待ち")]
    [InlineData(FuType.JantouPlayerWind, "自風の雀頭")]
    [InlineData(FuType.JantouRoundWind, "場風の雀頭")]
    [InlineData(FuType.JantouDragon, "三元牌の雀頭")]
    [InlineData(FuType.MinkoChunchan, "中張牌の明刻")]
    [InlineData(FuType.MinkoYaochu, "么九牌の明刻")]
    [InlineData(FuType.AnkoChunchan, "中張牌の暗刻")]
    [InlineData(FuType.AnkoYaochu, "么九牌の暗刻")]
    [InlineData(FuType.MinkanChunchan, "中張牌の明槓")]
    [InlineData(FuType.MinkanYaochu, "么九牌の明槓")]
    [InlineData(FuType.AnkanChunchan, "中張牌の暗槓")]
    [InlineData(FuType.AnkanYaochu, "么九牌の暗槓")]
    public void 有効な符種別_対応する文字列を返す(FuType fuType, string expected)
    {
        // Act
        var actual = fuType.ToStr();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 不正な符種別_ArgumentOutOfRangeExceptionが発生()
    {
        // Arrange
        var invalidFuType = (FuType)999;

        // Act
        var ex = Record.Exception(() => invalidFuType.ToStr());

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("fuType", ((ArgumentOutOfRangeException)ex).ParamName);
    }
}
