using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_HanClosedTests
{
    [Fact]
    public void 役を追加_正しい翻数を返す()
    {
        // Arrange
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu, Yaku.Sanshoku };

        // Act
        var yakuList = new YakuList(yakus);

        // Assert
        // Tanyao: 1翻, Pinfu: 1翻, Sanshoku: 2翻(面前時)
        Assert.Equal(4, yakuList.HanClosed);
    }
}
