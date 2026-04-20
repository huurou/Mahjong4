using static Mahjong.Lib.Game.Scoring.Tests.TestHelper;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class TenpaiCheckerImpl_IsKoutsuOnlyInAllInterpretationsTests
{
    private readonly TenpaiCheckerImpl checker_ = new();

    [Fact]
    public void 指定牌種が刻子専用_trueを返す()
    {
        // Arrange
        // 萬111 筒234 索567 萬99 索23 + 待ち 1/4 索 (kind 18, 21)
        // 萬1 (kind 0) は 3 枚すべてが刻子として使われる
        var hand = Hand(0, 0, 0, 10, 11, 12, 22, 23, 24, 8, 8, 19, 20);

        // Act
        var result = checker_.IsKoutsuOnlyInAllInterpretations(hand, EmptyCallList(), kind: 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 指定牌種が順子解釈も存在_falseを返す()
    {
        // Arrange
        // 萬 1122 33 456 筒 234 索 99 + ツモ 萬2 の状況
        // 萬 112233 は 「112233 の刻子解釈」と「123 123 の順子解釈」の両方が存在
        // kind 1 (萬2) を暗槓する場合、順子解釈があれば false にすべき
        // 手牌 (ツモ前 13 枚): 萬 112 2 33 345 筒 23 45 索 9 東 ? ここは複雑

        // シンプルに: 萬 11 22 33 456789 索 99 + 待ちは index ベース
        // 萬 112233 (kinds 0,0,1,1,2,2) + 萬 456789 (3,4,5,6,7,8) + 索 99 (26,26) = 14 枚 → 13 枚手牌にする
        // 萬 112233 456 789 索 99 9 で待ち 9索 単騎 → kind 0 (萬1) は
        //   解釈A: 111刻 / 222 (2つ足りない) → NG
        //   解釈B: 123 123 の順子 + 456 + 789 + 99雀頭 → 萬1が順子に使われる
        // 手牌 12 枚: 萬 112233456789 索 99 (13 枚) + 待ち 9索
        var hand = Hand(0, 0, 1, 1, 2, 2, 3, 4, 5, 6, 7, 8, 26);

        // Act: kind 0 (萬1) を指定。順子解釈があるので false
        var result = checker_.IsKoutsuOnlyInAllInterpretations(hand, EmptyCallList(), kind: 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ノーテン_falseを返す()
    {
        // Arrange
        var hand = Hand(0, 1, 3, 5, 10, 12, 14, 16, 20, 22, 24, 26, 33);

        // Act
        var result = checker_.IsKoutsuOnlyInAllInterpretations(hand, EmptyCallList(), kind: 0);

        // Assert
        Assert.False(result);
    }
}
