using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class YakuList_ToStringTests
{
    [Fact]
    public void 空のリスト_空文字列を返す()
    {
        // Arrange
        var yakuList = new YakuList();

        // Act
        var result = yakuList.ToString();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void 複数の役_空白区切りの文字列を返す()
    {
        // Arrange
        // Pinfu(7), Tanyao(8) の順にソートされる
        var yakuList = new YakuList([Yaku.Tanyao, Yaku.Pinfu]);

        // Act
        var result = yakuList.ToString();

        // Assert
        Assert.Equal($"{Yaku.Pinfu} {Yaku.Tanyao}", result);
    }
}
