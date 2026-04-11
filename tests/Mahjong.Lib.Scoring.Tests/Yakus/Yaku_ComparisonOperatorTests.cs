using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Yaku_ComparisonOperatorTests
{
    [Fact]
    public void 小なり演算子_Numberが小さい場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Yaku.Riichi < Yaku.Tanyao);
        Assert.False(Yaku.Tanyao < Yaku.Riichi);
        Assert.False(Yaku.Riichi < Yaku.Riichi);
    }

    [Fact]
    public void 大なり演算子_Numberが大きい場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Yaku.Tanyao > Yaku.Riichi);
        Assert.False(Yaku.Riichi > Yaku.Tanyao);
        Assert.False(Yaku.Riichi > Yaku.Riichi);
    }

    [Fact]
    public void 以下演算子_Number以下の場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Yaku.Riichi <= Yaku.Tanyao);
        Assert.True(Yaku.Riichi <= Yaku.Riichi);
        Assert.False(Yaku.Tanyao <= Yaku.Riichi);
    }

    [Fact]
    public void 以上演算子_Number以上の場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Yaku.Tanyao >= Yaku.Riichi);
        Assert.True(Yaku.Riichi >= Yaku.Riichi);
        Assert.False(Yaku.Riichi >= Yaku.Tanyao);
    }

    [Fact]
    public void Null比較_正しく処理される()
    {
        // Arrange
        Yaku? nullYaku = null;

        // Act & Assert
        Assert.True(nullYaku < Yaku.Riichi);
        Assert.False(nullYaku > Yaku.Riichi);
        Assert.True(nullYaku <= Yaku.Riichi);
        Assert.False(nullYaku >= Yaku.Riichi);
    }
}
