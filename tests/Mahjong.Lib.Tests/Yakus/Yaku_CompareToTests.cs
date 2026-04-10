using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class Yaku_CompareToTests
{
    [Fact]
    public void Nullと比較_1を返す()
    {
        // Arrange
        Yaku yaku = Yaku.Riichi;
        Yaku? other = null;

        // Act
        var result = yaku.CompareTo(other);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void 自分自身と比較_0を返す()
    {
        // Arrange
        Yaku yaku = Yaku.Riichi;
        Yaku other = Yaku.Riichi;

        // Act
        var result = yaku.CompareTo(other);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void 番号が小さい役と比較_正の値を返す()
    {
        // Arrange
        Yaku smallerYaku = Yaku.Riichi;
        Yaku largerYaku = Yaku.Tanyao;

        Assert.True(smallerYaku.Number < largerYaku.Number);

        // Act
        var result = largerYaku.CompareTo(smallerYaku);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void 番号が大きい役と比較_負の値を返す()
    {
        // Arrange
        Yaku smallerYaku = Yaku.Riichi;
        Yaku largerYaku = Yaku.Tanyao;

        Assert.True(smallerYaku.Number < largerYaku.Number);

        // Act
        var result = smallerYaku.CompareTo(largerYaku);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void 同一番号で翻数が異なる役_翻数で比較される()
    {
        // Arrange
        // Daisuushii(Number=49, HanClosed=13) と DaisuushiiDouble(Number=49, HanClosed=26)
        Yaku regular = Yaku.Daisuushii;
        Yaku doubleYakuman = Yaku.DaisuushiiDouble;

        Assert.Equal(regular.Number, doubleYakuman.Number);
        Assert.True(regular.HanClosed < doubleYakuman.HanClosed);

        // Act
        var result = regular.CompareTo(doubleYakuman);

        // Assert
        Assert.True(result < 0);
    }
}
