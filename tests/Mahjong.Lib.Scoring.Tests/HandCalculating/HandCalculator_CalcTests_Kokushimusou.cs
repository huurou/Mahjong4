using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void 国士無双_成立_役リストに国士無双が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Man1;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Kokushimusou, actual.YakuList);
    }

    [Fact]
    public void 国士無双十三面待ち_成立_役リストに国士無双十三面待ちが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Ton;
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Kokushimusou13menmachi, actual.YakuList);
    }

    [Fact]
    public void 国士無双十三面待ちダブル_成立_役リストに国士無双十三面待ちダブルが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Ton;
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Kokushimusou13menmachiDouble, actual.YakuList);
    }

    [Fact]
    public void 国士無双かつ天和_役リストに国士無双と天和のみが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTenhou = true, IsTsumo = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal([Yaku.Tenhou, Yaku.Kokushimusou], actual.YakuList);
    }

    [Fact]
    public void 国士無双かつ地和_役リストに国士無双と地和のみが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Man1;
        var winSituation = new WinSituation { IsChiihou = true, IsTsumo = true, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal([Yaku.Chiihou, Yaku.Kokushimusou], actual.YakuList);
    }

    [Fact]
    public void 国士無双かつ人和役満_役リストに国士無双と人和役満のみが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Man1;
        var winSituation = new WinSituation { IsRenhou = true, IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Equal([Yaku.RenhouYakuman, Yaku.Kokushimusou], actual.YakuList);
    }

    [Fact]
    public void 国士無双かつ役満でない人和_成立_役リストに国士無双のみが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Man1;
        var winSituation = new WinSituation { IsRenhou = true, IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Equal([Yaku.Kokushimusou], actual.YakuList);
    }
}
