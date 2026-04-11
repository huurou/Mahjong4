using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void ドラ_役リストにドラがドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123444456789");
        var winTile = TileKind.Sou4;
        var doraIndicators = new TileKindList(sou: "3");

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, doraIndicators: doraIndicators);

        // Assert
        Assert.Contains(Yaku.Dora, actual.YakuList);
        Assert.Equal(4, actual.YakuList.Count(x => x == Yaku.Dora));
    }

    [Fact]
    public void 裏ドラ_役リストに裏ドラが裏ドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123444456789");
        var winTile = TileKind.Sou4;
        var uradoraIndicators = new TileKindList(sou: "3");

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, uradoraIndicators: uradoraIndicators);

        // Assert
        Assert.Contains(Yaku.Uradora, actual.YakuList);
        Assert.Equal(4, actual.YakuList.Count(x => x == Yaku.Uradora));
    }

    [Fact]
    public void 赤ドラ_役リストに赤ドラが赤ドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123455556789");
        var winTile = TileKind.Sou1;
        var winSituation = new WinSituation { AkadoraCount = 2 };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Akadora, actual.YakuList);
        Assert.Equal(2, actual.YakuList.Count(x => x == Yaku.Akadora));
    }

    [Fact]
    public void ドラ_表示牌が複数_役リストにドラがドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123444456789");
        var winTile = TileKind.Sou4;
        var doraIndicators = new TileKindList(sou: "33");

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, doraIndicators: doraIndicators);

        // Assert
        Assert.Contains(Yaku.Dora, actual.YakuList);
        Assert.Equal(8, actual.YakuList.Count(x => x == Yaku.Dora));
    }

    [Fact]
    public void 裏ドラ_表示牌が複数_役リストに裏ドラが裏ドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123444456789");
        var winTile = TileKind.Sou4;
        var uradoraIndicators = new TileKindList(sou: "33");

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, uradoraIndicators: uradoraIndicators);

        // Assert
        Assert.Contains(Yaku.Uradora, actual.YakuList);
        Assert.Equal(8, actual.YakuList.Count(x => x == Yaku.Uradora));
    }

    [Fact]
    public void ドラ_副露にドラが含まれる_役リストにドラがドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123456789");
        var winTile = TileKind.Sou4;
        var doraIndicators = new TileKindList(sou: "3");
        var callList = new CallList([Call.Pon(sou: "444")]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, doraIndicators: doraIndicators, callList: callList);

        // Assert
        Assert.Contains(Yaku.Dora, actual.YakuList);
        Assert.Equal(4, actual.YakuList.Count(x => x == Yaku.Dora));
    }

    [Fact]
    public void 裏ドラ_副露に裏ドラが含まれる_役リストに裏ドラが裏ドラの個数含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123456789");
        var winTile = TileKind.Sou4;
        var uradoraIndicators = new TileKindList(sou: "3");
        var callList = new CallList([Call.Pon(sou: "444")]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, uradoraIndicators: uradoraIndicators, callList: callList);

        // Assert
        Assert.Contains(Yaku.Uradora, actual.YakuList);
        Assert.Equal(4, actual.YakuList.Count(x => x == Yaku.Uradora));
    }

    [Fact]
    public void 裏ドラ_該当牌が手牌に無い_役リストに裏ドラが含まれない()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "11123444456789");
        var winTile = TileKind.Sou4;
        // 裏ドラ表示牌が萬子1 → 実ドラは萬子2 だが手牌に存在しない
        var uradoraIndicators = new TileKindList(man: "1");

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, uradoraIndicators: uradoraIndicators);

        // Assert
        Assert.DoesNotContain(Yaku.Uradora, actual.YakuList);
    }
}
