using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_AddRangeTests
{
    [Fact]
    public void 複数役を追加_新しいYakuListに全て含まれる()
    {
        // Arrange
        var yakuList = new YakuList();
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu, Yaku.Sanshoku };

        // Act
        var newList = yakuList.AddRange(yakus);

        // Assert
        Assert.Empty(yakuList);
        Assert.Equal(3, newList.Count);
        Assert.Contains(Yaku.Tanyao, newList);
        Assert.Contains(Yaku.Pinfu, newList);
        Assert.Contains(Yaku.Sanshoku, newList);
    }

    [Fact]
    public void 空の役リストに追加_新しい役リストに全ての役が含まれる()
    {
        // Arrange
        var emptyList = new YakuList();
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu };

        // Act
        var result = emptyList.AddRange(yakus);

        // Assert
        Assert.Empty(emptyList);
        Assert.Equal(2, result.Count);
        Assert.Contains(Yaku.Tanyao, result);
        Assert.Contains(Yaku.Pinfu, result);
    }

    [Fact]
    public void 中身がある役リストに追加_既存と新規の役がすべて含まれる()
    {
        // Arrange
        var initialYakus = new List<Yaku> { Yaku.Tanyao };
        var yakuList = new YakuList(initialYakus);
        var additionalYakus = new List<Yaku> { Yaku.Pinfu, Yaku.Sanshoku };

        // Act
        var result = yakuList.AddRange(additionalYakus);

        // Assert
        Assert.Single(yakuList);
        Assert.Equal(3, result.Count);
        Assert.Contains(Yaku.Tanyao, result);
        Assert.Contains(Yaku.Pinfu, result);
        Assert.Contains(Yaku.Sanshoku, result);
    }
}
