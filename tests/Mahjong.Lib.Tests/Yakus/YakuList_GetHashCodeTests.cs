using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_GetHashCodeTests
{
    [Fact]
    public void 同じ内容_同じハッシュコードを返す()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);

        // Act
        var hashCode1 = yakuList1.GetHashCode();
        var hashCode2 = yakuList2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void 順序が異なる同じ内容_同じハッシュコードを返す()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Pinfu, Yaku.Tanyao]);

        // Act
        var hashCode1 = yakuList1.GetHashCode();
        var hashCode2 = yakuList2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void 異なる内容_異なるハッシュコードを返す()
    {
        // Arrange
        var yakuList1 = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);
        var yakuList2 = new YakuList([Yaku.Tanyao, Yaku.Sanshoku]);

        // Act
        var hashCode1 = yakuList1.GetHashCode();
        var hashCode2 = yakuList2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }
}
