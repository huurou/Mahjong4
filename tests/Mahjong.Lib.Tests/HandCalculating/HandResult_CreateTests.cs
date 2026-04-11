using Mahjong.Lib.Calls;
using Mahjong.Lib.HandCalculating;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.HandCalculating;

public class HandResult_CreateTests
{
    [Fact]
    public void 省略可能引数を全て省略_既定値で計算される()
    {
        // Arrange
        YakuList yakuList = [Yaku.Riichi];

        // Act
        var actual = HandResult.Create(yakuList);

        // Assert
        Assert.Null(actual.ErrorMessage);
        Assert.Equal(1, actual.Han);
    }

    [Fact]
    public void 副露ありで面前限定役のみ_役がないエラーが返される()
    {
        // Arrange: Pinfu は面前限定役 (HanOpen=0) のため副露ありでは翻数が0になる
        var callList = new CallList([Call.Chi(new TileKindList(man: "123"))]);

        // Act
        var actual = HandResult.Create([Yaku.Pinfu], callList: callList);

        // Assert
        Assert.Equal("役がありません。", actual.ErrorMessage);
        Assert.Equal(0, actual.Han);
    }
}
