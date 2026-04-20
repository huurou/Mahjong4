using static Mahjong.Lib.Game.Scoring.Tests.TestHelper;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class TenpaiCheckerImpl_IsTenpaiTests
{
    private readonly TenpaiCheckerImpl checker_ = new();

    [Fact]
    public void 両面待ちテンパイ_trueを返す()
    {
        // Arrange
        // 萬123 筒234 索567 筒789 + 筒5 (待ち 3/6 筒)
        // 13 枚手牌: 萬123 筒23 索567 筒789 筒5 (雀頭) → テンパイにならない
        // 平和両面待ち: 萬123 筒345 索678 萬99 索23 (待ち 1/4 索)
        var hand = Hand(0, 1, 2, 10, 11, 12, 20, 21, 22, 8, 8, 19, 20);

        // Act
        var result = checker_.IsTenpai(hand, EmptyCallList());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void シャンポン待ちテンパイ_trueを返す()
    {
        // Arrange
        // 萬111 筒234 索567 萬99 筒99 (待ち 9萬 / 9筒 のシャンポン)
        var hand = Hand(0, 0, 0, 10, 11, 12, 22, 23, 24, 8, 8, 17, 17);

        // Act
        var result = checker_.IsTenpai(hand, EmptyCallList());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 七対子テンパイ_trueを返す()
    {
        // Arrange
        // 萬1x2 萬3x2 筒2x2 筒7x2 索3x2 索8x2 東x1 (待ち 東単騎)
        var hand = Hand(0, 0, 2, 2, 10, 10, 15, 15, 20, 20, 25, 25, 27);

        // Act
        var result = checker_.IsTenpai(hand, EmptyCallList());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 国士無双テンパイ_trueを返す()
    {
        // Arrange
        // 国士無双 13 面待ち (幺九牌 13 種 + 任意 1 枚)
        var hand = Hand(0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33);

        // Act
        var result = checker_.IsTenpai(hand, EmptyCallList());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ノーテン手_falseを返す()
    {
        // Arrange
        // 雀頭なし + 面子バラバラ
        var hand = Hand(0, 1, 3, 5, 10, 12, 14, 16, 20, 22, 24, 26, 33);

        // Act
        var result = checker_.IsTenpai(hand, EmptyCallList());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 副露1つありテンパイ_trueを返す()
    {
        // Arrange
        // 副露 Pon 9萬 (kind 8) + 手牌 10 枚: 萬123 筒234 索567 筒1 (萬123 + 筒234 + 索567 + 9萬刻(副露) + 筒1単騎)
        var hand = Hand(0, 1, 2, 10, 11, 12, 22, 23, 24, 9);
        var callList = CallList(Pon(8));

        // Act
        var result = checker_.IsTenpai(hand, callList);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 副露1つありノーテン_falseを返す()
    {
        // Arrange
        // 副露 Pon 9萬 + 手牌 10 枚で面子がバラバラ
        var hand = Hand(0, 2, 4, 10, 12, 14, 20, 22, 24, 33);
        var callList = CallList(Pon(8));

        // Act
        var result = checker_.IsTenpai(hand, callList);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 副露2つありテンパイ_trueを返す()
    {
        // Arrange
        // 副露 Pon 9萬 + Pon 東 (kind 27) + 手牌 7 枚: 萬123 筒345 筒5 (雀頭筒5 + 萬123 + 筒345 + 9萬刻 + 東刻)
        // 手牌 萬123 筒345 筒5 = 7 枚、待ち筒5 シャンテン=0?
        // 面子: 萬123 (完成) + 筒345 (完成) + 筒5 (雀頭候補) → 雀頭単騎テンパイ
        var hand = Hand(0, 1, 2, 11, 12, 13, 13);
        var callList = CallList(Pon(8), Pon(27));

        // Act
        var result = checker_.IsTenpai(hand, callList);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 暗槓ありテンパイ_trueを返す()
    {
        // Arrange
        // 暗槓 東 (kind 27) + 手牌 10 枚: 萬123 筒234 索567 筒1 (筒1単騎待ち)
        var hand = Hand(0, 1, 2, 10, 11, 12, 22, 23, 24, 9);
        var callList = CallList(Ankan(27));

        // Act
        var result = checker_.IsTenpai(hand, callList);

        // Assert
        Assert.True(result);
    }
}
