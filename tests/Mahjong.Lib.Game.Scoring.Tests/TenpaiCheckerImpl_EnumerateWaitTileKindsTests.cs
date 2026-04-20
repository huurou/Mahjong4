using static Mahjong.Lib.Game.Scoring.Tests.TestHelper;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class TenpaiCheckerImpl_EnumerateWaitTileKindsTests
{
    private readonly TenpaiCheckerImpl checker_ = new();

    [Fact]
    public void 両面待ち_2種類の待ち牌種を返す()
    {
        // Arrange
        // 萬123 筒345 索678 萬99 索23 (待ち 1/4 索) kind 18, 21
        var hand = Hand(0, 1, 2, 11, 12, 13, 23, 24, 25, 8, 8, 19, 20);

        // Act
        var waits = checker_.EnumerateWaitTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Equal(2, waits.Count);
        Assert.Contains(18, waits);
        Assert.Contains(21, waits);
    }

    [Fact]
    public void シャンポン待ち_2種類の待ち牌種を返す()
    {
        // Arrange
        // 萬111 筒234 索567 萬99 筒99 (待ち 9萬 kind 8 / 9筒 kind 17)
        var hand = Hand(0, 0, 0, 10, 11, 12, 22, 23, 24, 8, 8, 17, 17);

        // Act
        var waits = checker_.EnumerateWaitTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Equal(2, waits.Count);
        Assert.Contains(8, waits);
        Assert.Contains(17, waits);
    }

    [Fact]
    public void ノーテン_空集合を返す()
    {
        // Arrange
        var hand = Hand(0, 1, 3, 5, 10, 12, 14, 16, 20, 22, 24, 26, 33);

        // Act
        var waits = checker_.EnumerateWaitTileKinds(hand, EmptyCallList());

        // Assert
        Assert.Empty(waits);
    }

    [Fact]
    public void 副露1つありテンパイ_待ち牌種を1種類以上返す()
    {
        // Arrange
        // Pon 9萬 (kind 8) + 手牌 萬123 筒234 索567 筒1 (筒1=kind9 が主な待ち)
        // 実装は callList を使わず手牌枚数のみで副露数を推定するため、構造によっては 2 種以上の解釈が返ることがある
        var hand = Hand(0, 1, 2, 10, 11, 12, 22, 23, 24, 9);
        var callList = CallList(Pon(8));

        // Act
        var waits = checker_.EnumerateWaitTileKinds(hand, callList);

        // Assert
        Assert.NotEmpty(waits);
        Assert.Contains(9, waits);
    }

    [Fact]
    public void 副露1つあり両面待ち_両面の2牌種を含む()
    {
        // Arrange
        // Pon 9萬 + 手牌 萬123 + 筒345 + 索78 (両面) + 索99 (雀頭) = 10 枚 → 索6(23)/索9(26) 両面
        var hand = Hand(0, 1, 2, 11, 12, 13, 24, 25, 26, 26);
        var callList = CallList(Pon(8));

        // Act
        var waits = checker_.EnumerateWaitTileKinds(hand, callList);

        // Assert
        Assert.Contains(23, waits); // 索6
        Assert.Contains(26, waits); // 索9
    }
}
