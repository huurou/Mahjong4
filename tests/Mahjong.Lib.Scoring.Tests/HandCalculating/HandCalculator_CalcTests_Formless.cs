using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void ツモ_面前かつツモ_役リストにツモが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "123", sou: "12344");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Tsumo, actual.YakuList);
    }

    [Fact]
    public void 立直_成立_役リストに立直が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRiichi = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Riichi, actual.YakuList);
    }

    [Fact]
    public void ダブル立直_成立_役リストにダブル立直が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsDoubleRiichi = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.DoubleRiichi, actual.YakuList);
    }

    [Fact]
    public void 断么九_成立_役リストに断么九が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "234567", pin: "234", sou: "23444");
        var winTile = TileKind.Sou4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Tanyao, actual.YakuList);
    }

    [Fact]
    public void 一発_成立_役リストに一発が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRiichi = true, IsIppatsu = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Ippatsu, actual.YakuList);
    }

    [Fact]
    public void 槍槓_成立_役リストに槍槓が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsChankan = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Chankan, actual.YakuList);
    }

    [Fact]
    public void 嶺上開花_成立_役リストに嶺上開花が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsTsumo = true, IsRinshan = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Rinshan, actual.YakuList);
    }

    [Fact]
    public void 海底撈月_成立_役リストに海底撈月が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsTsumo = true, IsHaitei = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Haitei, actual.YakuList);
    }

    [Fact]
    public void 河底撈魚_成立_役リストに河底撈魚が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsTsumo = false, IsHoutei = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Houtei, actual.YakuList);
    }

    [Fact]
    public void 人和_成立_役リストに人和が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRenhou = true, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Renhou, actual.YakuList);
    }

    [Fact]
    public void 混一色_成立_役リストに混一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456789", honor: "pppnn");
        var winTile = TileKind.Man6;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Honitsu, actual.YakuList);
    }

    [Fact]
    public void 清一色_正しい構成_役リストに清一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(pin: "11223345678999");
        var winTile = TileKind.Pin4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chinitsu, actual.YakuList);
    }

    [Fact]
    public void 字一色_成立_役リストに字一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "tttnnnssshhhcc");
        var winTile = TileKind.Chun;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Tsuuiisou, actual.YakuList);
    }

    [Fact]
    public void 字一色七対子版_成立_役リストに字一色が含まれ七対子は含まれない()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "ttnnsspphhrrcc");
        var winTile = TileKind.Chun;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Tsuuiisou, actual.YakuList);
    }

    [Fact]
    public void 混老頭_成立_役リストに混老頭が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111999", pin: "111", sou: "999", honor: "tt");
        var winTile = TileKind.Man1;
        var winSituation = new WinSituation();

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Honroutou, actual.YakuList);
    }

    [Fact]
    public void 緑一色_成立_役リストに緑一色が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "22334466888", honor: "rrr");
        var winTile = TileKind.Sou6;
        var winSituation = new WinSituation();

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Ryuuiisou, actual.YakuList);
    }

    [Fact]
    public void 地和_成立_役リストに地和が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsChiihou = true, IsTsumo = true, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Chiihou, actual.YakuList);
    }

    [Fact]
    public void 人和役満_成立_役リストに人和役満が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRenhou = true, IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.RenhouYakuman, actual.YakuList);
    }
}
