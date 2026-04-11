using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class YakuList_EqualsTests
{
    [Fact]
    public void 同じ役を含む_trueを返す()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);

        // Act & Assert
        Assert.True(yakuList1.Equals(yakuList2));
        Assert.True(yakuList2.Equals(yakuList1));
        Assert.Equal(yakuList1, yakuList2);
    }

    [Fact]
    public void 異なる役を含む_falseを返す()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Tanyao, Yaku.Sanshoku]);

        // Act & Assert
        Assert.False(yakuList1.Equals(yakuList2));
        Assert.False(yakuList2.Equals(yakuList1));
        Assert.NotEqual(yakuList1, yakuList2);
    }

    [Fact]
    public void 順序が異なる同じ役_等しいと判定される()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Pinfu, Yaku.Tanyao]);

        // Act & Assert
        Assert.True(yakuList1.Equals(yakuList2));
        Assert.True(yakuList2.Equals(yakuList1));
        Assert.Equal(yakuList1, yakuList2);
        Assert.Equal(yakuList1.GetHashCode(), yakuList2.GetHashCode());
    }

    [Fact]
    public void Nullとの比較_falseを返す()
    {
        // Arrange
        var yakuList = new YakuList([Yaku.Tanyao]);
        YakuList? nullYakuList = null;

        // Act & Assert
        Assert.False(yakuList.Equals(nullYakuList));
    }

    [Fact]
    public void 自分自身との比較_trueを返す()
    {
        // Arrange
        var yakuList = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);

        // Act & Assert
        Assert.True(yakuList.Equals(yakuList));
    }
}
