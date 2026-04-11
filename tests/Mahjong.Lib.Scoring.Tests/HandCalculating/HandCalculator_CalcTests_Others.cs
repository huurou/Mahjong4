using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void 流し満貫_成立_役リストに流し満貫のみが含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "13579", pin: "135", sou: "12344");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsNagashimangan = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal([Yaku.Nagashimangan], actual.YakuList);
    }

    [Fact]
    public void 七対子_成立_役リストに七対子が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "1133", pin: "2244", sou: "3355", honor: "tt");
        var winTile = TileKind.Sou5;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Contains(Yaku.Chiitoitsu, actual.YakuList);
    }

    [Fact]
    public void 大車輪_成立_役リストに大車輪が含まれる()
    {
        // Arrange
        var tileKindList = new TileKindList(pin: "22334455667788");
        var winTile = TileKind.Pin2;
        var gameRules = new GameRules { DaisharinEnabled = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, gameRules: gameRules);

        // Assert
        Assert.Contains(Yaku.Daisharin, actual.YakuList);
    }

    [Fact]
    public void 複数の役リストに取れる_点数が高い方が返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "112233", pin: "44778899");
        var winTile = TileKind.Pin4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal([Yaku.Ryanpeikou], actual.YakuList);
    }

    [Fact]
    public void 役満かつ数え役満ルールSanbaiman_役満点数が維持される()
    {
        // Arrange: 大三元 (役満) + 数え役満をSanbaimanに圧縮するルール
        // 子ロン想定 (PlayerWind=South)、本来の子ロン役満=32000、Sanbaimanに圧縮されると24000
        var tileKindList = new TileKindList(man: "111", honor: "tthhhrrrccc");
        var winTile = TileKind.Chun;
        var winSituation = new WinSituation { PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Sanbaiman };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation, gameRules: gameRules);

        // Assert: 役満の点数 (子ロン 32000) が Sanbaiman (24000) に圧縮されないこと
        Assert.Contains(Yaku.Daisangen, actual.YakuList);
        Assert.Equal(32000, actual.Score.Main);
    }

    [Fact]
    public void 役無し_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", honor: "ppphh");
        var winTile = TileKind.Man3;
        var callList = new CallList([Call.Chi(new(man: "456")), Call.Pon(new(pin: "777"))]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Equal("役がありません。", actual.ErrorMessage);
    }
}
