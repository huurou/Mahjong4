using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void 平和_成立_役リストに平和が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var winTile = TileKind.Pin4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Pinfu, actual.YakuList);
    }

    [Fact]
    public void 混全帯么九_成立_役リストに混全帯么九が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123789", pin: "123789", honor: "nn");
        var winTile = TileKind.Man1;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chanta, actual.YakuList);
    }

    [Fact]
    public void 純全帯么九_成立_役リストに純全帯么九が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123789", pin: "123", sou: "11123");
        var winTile = TileKind.Man1;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Junchan, actual.YakuList);
    }

    [Fact]
    public void 一盃口_成立_役リストに一盃口が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "112233", pin: "234", sou: "23444");
        var winTile = TileKind.Sou4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Iipeikou, actual.YakuList);
    }

    [Fact]
    public void 三色同順_3色同じ順子_役リストに三色同順が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12345677");
        var winTile = TileKind.Sou7;
        var winSituation = new WinSituation();

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Sanshoku, actual.YakuList);
    }
}
