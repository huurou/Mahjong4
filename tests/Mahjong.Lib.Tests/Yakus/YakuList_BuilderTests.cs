using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.Yakus;

public class YakuList_BuilderTests
{
    [Fact]
    public void Create_正しくYakuListを生成する()
    {
        // Arrange
        var yakus = new Yaku[] { Yaku.Tanyao, Yaku.Pinfu };

        // Act
        var yakuList = YakuList.YakuListBuilder.Create(yakus);

        // Assert
        Assert.Equal(2, yakuList.Count);
        Assert.Contains(Yaku.Tanyao, yakuList);
        Assert.Contains(Yaku.Pinfu, yakuList);
    }
}
