using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.HandCalculating;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void 対々和_成立_役リストに対々和が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(sou: "333444", honor: "cc");
        var callList = new CallList([Call.Pon(new(man: "111")), Call.Pon(new(pin: "222"))]);
        var winTile = TileKind.Sou3;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Contains(Yaku.Toitoihou, actual.YakuList);
    }

    [Fact]
    public void 三暗刻_成立_役リストに三暗刻が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "222", sou: "333345", honor: "cc");
        var winTile = TileKind.Sou5;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Sanankou, actual.YakuList);
    }

    [Fact]
    public void 三色同刻_成立_役リストに三色同刻が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "333", pin: "333", sou: "333345", honor: "cc");
        var winTile = TileKind.Sou5;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Sanshokudoukou, actual.YakuList);
    }

    [Fact]
    public void 小三元_成立_役リストに小三元が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "234", honor: "hhhrrrcc");
        var winTile = TileKind.Chun;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Shousangen, actual.YakuList);
    }

    [Fact]
    public void 白_成立_役リストに白が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "hhh");
        var winTile = TileKind.Haku;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Haku, actual.YakuList);
    }

    [Fact]
    public void 發_成立_役リストに發が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "rrr");
        var winTile = TileKind.Hatsu;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Hatsu, actual.YakuList);
    }

    [Fact]
    public void 中_成立_役リストに中が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "ccc");
        var winTile = TileKind.Chun;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chun, actual.YakuList);
    }

    [Fact]
    public void 自風牌東_成立_役リストに自風牌東が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "ttt");
        var winTile = TileKind.Ton;
        var winSituation = new WinSituation { PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.PlayerWindEast, actual.YakuList);
    }

    [Fact]
    public void 自風牌南_成立_役リストに自風牌南が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "nnn");
        var winTile = TileKind.Nan;
        var winSituation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.PlayerWindSouth, actual.YakuList);
    }

    [Fact]
    public void 自風牌西_成立_役リストに自風牌西が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "sss");
        var winTile = TileKind.Sha;
        var winSituation = new WinSituation { PlayerWind = Wind.West, RoundWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.PlayerWindWest, actual.YakuList);
    }

    [Fact]
    public void 自風牌北_成立_役リストに自風牌北が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "ppp");
        var winTile = TileKind.Pei;
        var winSituation = new WinSituation { PlayerWind = Wind.North, RoundWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.PlayerWindNorth, actual.YakuList);
    }

    [Fact]
    public void 場風牌東_成立_役リストに場風牌東が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "ttt");
        var winTile = TileKind.Ton;
        var winSituation = new WinSituation { RoundWind = Wind.East, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.RoundWindEast, actual.YakuList);
    }

    [Fact]
    public void 場風牌南_成立_役リストに場風牌南が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "nnn");
        var winTile = TileKind.Nan;
        var winSituation = new WinSituation { RoundWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.RoundWindSouth, actual.YakuList);
    }

    [Fact]
    public void 場風牌西_成立_役リストに場風牌西が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "sss");
        var winTile = TileKind.Sha;
        var winSituation = new WinSituation { RoundWind = Wind.West, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.RoundWindWest, actual.YakuList);
    }

    [Fact]
    public void 場風牌北_成立_役リストに場風牌北が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "123", sou: "12344", honor: "ppp");
        var winTile = TileKind.Pei;
        var winSituation = new WinSituation { RoundWind = Wind.North, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.RoundWindNorth, actual.YakuList);
    }

    [Fact]
    public void 大三元_成立_役リストに大三元が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", honor: "tthhhrrrccc");
        var winTile = TileKind.Chun;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Daisangen, actual.YakuList);
    }

    [Fact]
    public void 小四喜_成立_役リストに小四喜が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", honor: "tttnnnssspp");
        var winTile = TileKind.Pei;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Shousuushii, actual.YakuList);
    }

    [Fact]
    public void 大四喜_成立_役リストに大四喜が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "tttnnnssspppcc");
        var winTile = TileKind.Chun;
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Daisuushii, actual.YakuList);
    }

    [Fact]
    public void 大四喜ダブル_役リストに大四喜ダブルが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "tttnnnssspppcc");
        var winTile = TileKind.Chun;
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.DaisuushiiDouble, actual.YakuList);
    }

    [Fact]
    public void 九蓮宝燈_成立_役リストに九蓮宝燈が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11123456789999");
        var winTile = TileKind.Man1;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chuurenpoutou, actual.YakuList);
    }

    [Fact]
    public void 純正九蓮宝燈_成立_役リストに純正九蓮宝燈が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11123456789999");
        var winTile = TileKind.Man9;
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.JunseiChuurenpoutou, actual.YakuList);
    }

    [Fact]
    public void 純正九蓮宝燈ダブル_成立_役リストに純正九蓮宝燈が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11123456789999");
        var winTile = TileKind.Man9;
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.JunseiChuurenpoutouDouble, actual.YakuList);
    }

    [Fact]
    public void 四暗刻_成立_役リストに四暗刻が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "222", sou: "333", honor: "hhhpp");
        var winTile = TileKind.Haku;
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Contains(Yaku.Suuankou, actual.YakuList);
    }

    [Fact]
    public void 四暗刻_四暗刻単騎だがダブル役満無効_役リストに四暗刻が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "222", sou: "333", honor: "hhhpp");
        var winTile = TileKind.Haku;
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Suuankou, actual.YakuList);
    }

    [Fact]
    public void 四暗刻単騎_成立_役リストに四暗刻単騎が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111", pin: "222", sou: "333", honor: "hhhpp");
        var winTile = TileKind.Pei;
        var winSituation = new WinSituation { IsTsumo = false };
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.SuuankouTanki, actual.YakuList);
    }

    [Fact]
    public void 清老頭_成立_役リストに清老頭が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111999", pin: "11199", sou: "999");
        var winTile = TileKind.Pin9;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chinroutou, actual.YakuList);
    }

    [Fact]
    public void 三槓子_成立_役リストに三槓子が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", sou: "99");
        var winTile = TileKind.Sou9;
        var callList = new CallList([Call.Ankan(new(man: "5555")), Call.Minkan(new(pin: "5555")), Call.Ankan(new(sou: "5555"))]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Contains(Yaku.Sankantsu, actual.YakuList);
    }

    [Fact]
    public void 四槓子_成立_役リストに四槓子が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(honor: "cc");
        var winTile = TileKind.Chun;
        var callList = new CallList([Call.Ankan(new(man: "1111")), Call.Minkan(new(pin: "1111")), Call.Ankan(new(sou: "1111")), Call.Minkan(new(honor: "hhhh"))]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Contains(Yaku.Suukantsu, actual.YakuList);
    }
}
