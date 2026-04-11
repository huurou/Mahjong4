using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void 天和_成立_役リストに天和が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456789", pin: "111", sou: "22");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ九蓮宝燈_天和と九蓮宝燈返す()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11123456789999");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Chuurenpoutou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ四暗刻_役リストに天和と四暗刻が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "22333", sou: "555", honor: "ccc");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Suuankou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ大三元_役リストに天和と大三元が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "22", honor: "hhhrrrccc");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Daisangen, actual.YakuList);
    }

    [Fact]
    public void 天和かつ小四喜_役リストに天和と小四喜が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", honor: "tttnnnssspp");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Shousuushii, actual.YakuList);
    }

    [Fact]
    public void 天和かつ大四喜_ダブル役満有効_役リストに天和と大四喜ダブルが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11", honor: "tttnnnsssppp");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.DaisuushiiDouble, actual.YakuList);
    }

    [Fact]
    public void 天和かつ大四喜_ダブル役満無効_役リストに天和と大四喜が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11", honor: "tttnnnsssppp");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Daisuushii, actual.YakuList);
    }

    [Fact]
    public void 天和かつ緑一色_役リストに天和と緑一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "22334466888", honor: "rrr");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Ryuuiisou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ字一色_成立_役リストに天和と字一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "tttnnnssshhhrr");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Tsuuiisou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ清老頭_役リストに天和と清老頭が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111999", pin: "111999", sou: "11");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };
        var gameRules = new GameRules();

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Chinroutou, actual.YakuList);
    }

    [Fact]
    public void 天和かつ大車輪_役リストに天和と大車輪が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(pin: "22334455667788");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTsumo = true, IsTenhou = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { DaisharinEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
        Assert.Contains(Yaku.Daisharin, actual.YakuList);
    }
}
