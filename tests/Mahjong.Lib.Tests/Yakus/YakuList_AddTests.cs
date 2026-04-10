using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_AddTests
{
    [Fact]
    public void 役を追加_新しいYakuListに役が含まれる()
    {
        // Arrange
        var yakuList = new YakuList();
        var yaku = Yaku.Tanyao;

        // Act
        var newList = yakuList.Add(yaku);

        // Assert
        Assert.Empty(yakuList);
        Assert.Single(newList);
        Assert.Contains(yaku, newList);
    }

    [Fact]
    public void 重複する役を追加_全ての役が含まれる()
    {
        // Arrange
        var yakuList = new YakuList([Yaku.Tanyao]);

        // Act
        var result = yakuList.Add(Yaku.Tanyao);

        // Assert
        Assert.Single(yakuList);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Count(x => x == Yaku.Tanyao));
    }
}
