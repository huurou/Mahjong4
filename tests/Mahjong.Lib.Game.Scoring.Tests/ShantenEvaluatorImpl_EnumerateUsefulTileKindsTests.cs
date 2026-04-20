using static Mahjong.Lib.Game.Scoring.Tests.TestHelper;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class ShantenEvaluatorImpl_EnumerateUsefulTileKindsTests
{
    private readonly ShantenEvaluatorImpl evaluator_ = new();

    [Fact]
    public void テンパイ両面_待ち2種類を返す()
    {
        // Arrange
        // 萬123 筒345 索678 萬99 索23 (待ち 1索=18 / 4索=21)
        var hand = Hand(0, 1, 2, 11, 12, 13, 23, 24, 25, 8, 8, 19, 20);

        // Act
        var useful = evaluator_.EnumerateUsefulTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Equal(2, useful.Count);
        Assert.Contains(18, useful);
        Assert.Contains(21, useful);
    }

    [Fact]
    public void 一シャンテン_シャンテンを減らす牌種を複数返す()
    {
        // Arrange
        // 萬123 筒345 索678 萬9 筒9 索2 索3
        // シャンテン 1、9萬/9筒/1索/4索 を引けば 0 シャンテン
        var hand = Hand(0, 1, 2, 11, 12, 13, 23, 24, 25, 8, 17, 19, 20);

        // Act
        var useful = evaluator_.EnumerateUsefulTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Contains(8, useful);
        Assert.Contains(17, useful);
        Assert.Contains(18, useful);
        Assert.Contains(21, useful);
    }

    [Fact]
    public void 和了形_空集合を返す()
    {
        // Arrange
        // 萬123 筒123 索123 萬456 萬99 = 14 枚和了形
        var hand = Hand(0, 1, 2, 9, 10, 11, 18, 19, 20, 3, 4, 5, 8, 8);

        // Act
        var useful = evaluator_.EnumerateUsefulTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Empty(useful);
    }

    [Fact]
    public void 副露1つありテンパイ_待ち牌種を返す()
    {
        // Arrange
        // Pon 9萬 + 手牌10枚 (萬123 筒234 索567 筒1)
        var hand = Hand(0, 1, 2, 10, 11, 12, 22, 23, 24, 9);
        var callList = CallList(Pon(8));

        // Act
        var useful = evaluator_.EnumerateUsefulTileKinds(hand, callList);

        // Assert
        Assert.NotEmpty(useful);
        Assert.Contains(9, useful);
    }

    [Fact]
    public void すでに4枚保持している牌種_有効牌集合に含まれない()
    {
        // Arrange
        // 萬1111 筒23 索45 筒678 索789 (4枚所持 + 雀頭候補 + 順子候補)
        // 1萬 (kind 0) を4枚所持、有効牌として返ってはいけない
        var hand = Hand(0, 0, 0, 0, 10, 11, 21, 22, 14, 15, 16, 24, 25);

        // Act
        var useful = evaluator_.EnumerateUsefulTileKinds(hand, EmptyCallList());

        // Assert
        Assert.DoesNotContain(0, useful);
    }
}
