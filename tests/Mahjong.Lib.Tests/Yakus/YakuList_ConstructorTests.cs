using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_ConstructorTests
{
    [Fact]
    public void 空のYakuList作成_要素がない_成功する()
    {
        // Arrange & Act
        var yakuList = new YakuList();

        // Assert
        Assert.Empty(yakuList);
        Assert.Equal(0, yakuList.HanOpen);
        Assert.Equal(0, yakuList.HanClosed);
    }

    [Fact]
    public void コレクション指定コンストラクタ_役が追加される_成功する()
    {
        // Arrange
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu };

        // Act
        var yakuList = new YakuList(yakus);

        // Assert
        Assert.Equal(2, yakuList.Count);
        Assert.Contains(Yaku.Tanyao, yakuList);
        Assert.Contains(Yaku.Pinfu, yakuList);
    }

    [Fact]
    public void コレクション指定コンストラクタ_順不同の入力でもソートされる()
    {
        // Arrange
        // Tanyao(8), Pinfu(7), Riichi(1)
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu, Yaku.Riichi };

        // Act
        var yakuList = new YakuList(yakus);

        // Assert
        var items = yakuList.ToList();
        Assert.Equal(Yaku.Riichi, items[0]);    // Number: 1
        Assert.Equal(Yaku.Pinfu, items[1]);     // Number: 7
        Assert.Equal(Yaku.Tanyao, items[2]);    // Number: 8
    }
}
