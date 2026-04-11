using Mahjong.Lib.Scoring.Yakus;
using System.Collections;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class YakuList_GetEnumeratorTests
{
    [Fact]
    public void 列挙可能_正しく列挙できる()
    {
        // Arrange
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu };
        var yakuList = new YakuList(yakus);

        // Act
        var enumerated = new List<Yaku>();
        foreach (var yaku in yakuList)
        {
            enumerated.Add(yaku);
        }

        // Assert
        Assert.Equal(2, enumerated.Count);
        Assert.Contains(Yaku.Tanyao, enumerated);
        Assert.Contains(Yaku.Pinfu, enumerated);
    }

    [Fact]
    public void IEnumerable実装_正しく列挙できる()
    {
        // Arrange
        var yakus = new List<Yaku> { Yaku.Tanyao, Yaku.Pinfu };
        var yakuList = new YakuList(yakus);
        var enumerable = (IEnumerable)yakuList;

        // Act
        var enumerated = new List<Yaku>();
        foreach (var yaku in enumerable)
        {
            enumerated.Add((Yaku)yaku);
        }

        // Assert
        Assert.Equal(2, enumerated.Count);
        Assert.Contains(Yaku.Tanyao, enumerated);
        Assert.Contains(Yaku.Pinfu, enumerated);
    }
}
