using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.ScoreCalcValidation.Analysing.AgariInfos;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Inits;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.ScoreCalcValidation.Tests.Analysing.AgariInfos;

public class AgariInfoBuildService_BuildTests
{
    private static readonly Dictionary<int, Yaku> 役番号とYaku_ = new()
    {
        { 0, Yaku.Tsumo },
        { 1, Yaku.Riichi },
        { 2, Yaku.Ippatsu },
        { 3, Yaku.Chankan },
        { 4, Yaku.Rinshan },
        { 5, Yaku.Haitei },
        { 6, Yaku.Houtei },
        { 7, Yaku.Pinfu },
        { 8, Yaku.Tanyao },
        { 9, Yaku.Iipeikou },
        { 10, Yaku.PlayerWindEast },
        { 11, Yaku.PlayerWindSouth },
        { 12, Yaku.PlayerWindWest },
        { 13, Yaku.PlayerWindNorth },
        { 14, Yaku.RoundWindEast },
        { 15, Yaku.RoundWindSouth },
        { 16, Yaku.RoundWindWest },
        { 17, Yaku.RoundWindNorth },
        { 18, Yaku.Haku },
        { 19, Yaku.Hatsu },
        { 20, Yaku.Chun },
        { 22, Yaku.Chiitoitsu },
        { 23, Yaku.Chanta },
        { 24, Yaku.Ittsuu },
        { 25, Yaku.Sanshoku },
        { 26, Yaku.Sanshokudoukou },
        { 27, Yaku.Sankantsu },
        { 28, Yaku.Toitoihou },
        { 29, Yaku.Sanankou },
        { 30, Yaku.Shousangen },
        { 31, Yaku.Honroutou },
        { 32, Yaku.Ryanpeikou },
        { 33, Yaku.Junchan },
        { 34, Yaku.Honitsu },
        { 35, Yaku.Chinitsu },
        { 39, Yaku.Daisangen },
        { 40, Yaku.Suuankou },
        { 41, Yaku.SuuankouTanki },
        { 42, Yaku.Tsuuiisou },
        { 43, Yaku.Ryuuiisou },
        { 44, Yaku.Chinroutou },
        { 45, Yaku.Chuurenpoutou },
        { 46, Yaku.JunseiChuurenpoutou },
        { 47, Yaku.Kokushimusou },
        { 48, Yaku.Kokushimusou13menmachi },
        { 49, Yaku.Daisuushii },
        { 50, Yaku.Shousuushii },
        { 51, Yaku.Suukantsu },
    };

    public static TheoryData<int> 全役番号()
    {
        return [.. 役番号とYaku_.Keys];
    }

    [Theory]
    [MemberData(nameof(全役番号))]
    public void 非役満の通常役番号_対応するYakuに変換される(int number)
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 1000, ManganType.None,
            [new YakuInfo(number, 1)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.Contains(役番号とYaku_[number], actual.YakuList);
    }

    [Fact]
    public void ドラ役番号52_翻数分リストに追加される()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 3000, ManganType.None,
            [new YakuInfo(52, 3)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.Equal(3, actual.YakuList.Count(x => x == Yaku.Dora));
    }

    [Fact]
    public void 裏ドラ役番号53_翻数分リストに追加される()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 3000, ManganType.None,
            [new YakuInfo(53, 2)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.Equal(2, actual.YakuList.Count(x => x == Yaku.Uradora));
    }

    [Fact]
    public void 赤ドラ役番号54_翻数分リストに追加される()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 3000, ManganType.None,
            [new YakuInfo(54, 1)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 1,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.Single(actual.YakuList, x => x == Yaku.Akadora);
    }

    [Fact]
    public void ダブルリーチ役番号21_フラグが立つ()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 1000, ManganType.None,
            [new YakuInfo(21, 2)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.True(actual.WinSituation.IsDoubleRiichi);
        Assert.Contains(Yaku.DoubleRiichi, actual.YakuList);
    }

    [Fact]
    public void 人和役満番号36_RenhouYakumanに変換される()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 32000, ManganType.Yakuman,
            [],
            [36],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.True(actual.WinSituation.IsRenhou);
        Assert.Contains(Yaku.RenhouYakuman, actual.YakuList);
    }

    [Fact]
    public void 天和役満番号37_フラグが立つ()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 48000, ManganType.Yakuman,
            [],
            [37],
            [], [],
            IsTsumo: true,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.True(actual.WinSituation.IsTenhou);
        Assert.Contains(Yaku.Tenhou, actual.YakuList);
    }

    [Fact]
    public void 地和役満番号38_フラグが立つ()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 32000, ManganType.Yakuman,
            [],
            [38],
            [], [],
            IsTsumo: true,
            AkadoraCount: 0,
            Who: 1);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.True(actual.WinSituation.IsChiihou);
        Assert.Contains(Yaku.Chiihou, actual.YakuList);
    }

    [Fact]
    public void 状況役すべて_WinSituationに反映される()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 1000, ManganType.None,
            [
                new YakuInfo(1, 1), // Riichi
                new YakuInfo(2, 1), // Ippatsu
                new YakuInfo(3, 1), // Chankan
                new YakuInfo(4, 1), // Rinshan
                new YakuInfo(5, 1), // Haitei
                new YakuInfo(6, 1), // Houtei
            ],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.True(actual.WinSituation.IsRiichi);
        Assert.True(actual.WinSituation.IsIppatsu);
        Assert.True(actual.WinSituation.IsChankan);
        Assert.True(actual.WinSituation.IsRinshan);
        Assert.True(actual.WinSituation.IsHaitei);
        Assert.True(actual.WinSituation.IsHoutei);
    }

    [Fact]
    public void 和了者と親番の差分_自風が計算される()
    {
        // Arrange: oya=1, who=2 → (2+4-1)%4=1 → South
        var init = new Init(0, 0, Wind.East, 1);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 1000, ManganType.None,
            [],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 2);

        // Act
        var actual = AgariInfoBuildService.Build("g1", init, agari);

        // Assert
        Assert.Equal(Wind.South, actual.WinSituation.PlayerWind);
    }

    [Fact]
    public void 未知の役番号_例外が発生する()
    {
        // Arrange
        var init = new Init(0, 0, Wind.East, 0);
        var agari = new Agari(
            [],
            [],
            TileKind.Man1,
            30, 1000, ManganType.None,
            [new YakuInfo(99, 1)],
            [],
            [], [],
            IsTsumo: false,
            AkadoraCount: 0,
            Who: 0);

        // Act
        var exception = Record.Exception(() => AgariInfoBuildService.Build("g1", init, agari));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }
}
