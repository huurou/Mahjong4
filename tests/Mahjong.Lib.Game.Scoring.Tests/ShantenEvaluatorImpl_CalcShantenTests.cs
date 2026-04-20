using static Mahjong.Lib.Game.Scoring.Tests.TestHelper;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class ShantenEvaluatorImpl_CalcShantenTests
{
    private readonly ShantenEvaluatorImpl evaluator_ = new();

    [Fact]
    public void 和了形14枚_マイナス1を返す()
    {
        // Arrange
        // 萬123 筒123 索123 萬456 萬99 = 14 枚和了形
        var hand = Hand(0, 1, 2, 9, 10, 11, 18, 19, 20, 3, 4, 5, 8, 8);

        // Act
        var shanten = evaluator_.CalcShanten(hand, EmptyCallList());

        // Assert
        Assert.Equal(-1, shanten);
    }

    [Fact]
    public void テンパイ13枚_0を返す()
    {
        // Arrange
        // 萬123 筒345 索678 萬99 索23 (待ち 1/4 索)
        var hand = Hand(0, 1, 2, 11, 12, 13, 23, 24, 25, 8, 8, 19, 20);

        // Act
        var shanten = evaluator_.CalcShanten(hand, EmptyCallList());

        // Assert
        Assert.Equal(0, shanten);
    }

    [Fact]
    public void 一シャンテン13枚_1を返す()
    {
        // Arrange
        // 萬123 筒345 索678 萬9 筒9 索2 索3 (対子がなく 1 向聴)
        var hand = Hand(0, 1, 2, 11, 12, 13, 23, 24, 25, 8, 17, 19, 20);

        // Act
        var shanten = evaluator_.CalcShanten(hand, EmptyCallList());

        // Assert
        Assert.Equal(1, shanten);
    }

    [Fact]
    public void 副露1つありテンパイ_0を返す()
    {
        // Arrange
        // Pon 9萬 (kind 8) + 手牌10枚 (萬123 筒234 索567 筒1) → テンパイ
        var hand = Hand(0, 1, 2, 10, 11, 12, 22, 23, 24, 9);
        var callList = CallList(Pon(8));

        // Act
        var shanten = evaluator_.CalcShanten(hand, callList);

        // Assert
        Assert.Equal(0, shanten);
    }
}
