using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.AgariInfos;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Validating;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests.Validating;

public class CalcValidateService_ValidateTests
{
    private readonly CalcValidateService calcValidateService_ = new(NullLogger<CalcValidateService>.Instance);

    private static GameRules TenhouRules()
    {
        return new()
        {
            KuitanEnabled = true,
            DoubleYakumanEnabled = false,
            KazoeLimit = KazoeLimit.Limited,
            KiriageEnabled = false,
            PinzumoEnabled = true,
            RenhouAsYakumanEnabled = true,
            DaisharinEnabled = false,
        };
    }

    private static AgariInfo BuildExpected(TileKindList hand, TileKind winTile, WinSituation situation, ManganType manganType = ManganType.None)
    {
        var result = HandCalculator.Calc(hand, winTile, winSituation: situation, gameRules: TenhouRules());
        var totalScore =
            situation.IsTsumo && situation.IsDealer ? result.Score.Main * 3
            : situation.IsTsumo && !situation.IsDealer ? result.Score.Main + result.Score.Sub * 2
            : result.Score.Main;
        return new AgariInfo(
            "g1", 0, 0,
            hand,
            winTile,
            [],
            [], [],
            situation,
            result.Fu,
            result.Han,
            totalScore,
            result.YakuList,
            manganType);
    }

    [Fact]
    public void 子ロン_完全一致_IsSuccessがtrue()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var agariInfo = BuildExpected(hand, TileKind.Pin4, situation);

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Fact]
    public void 親ツモ_完全一致_IsSuccessがtrue()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation
        {
            IsTsumo = true,
            PlayerWind = Wind.East,
            RoundWind = Wind.East,
        };
        var agariInfo = BuildExpected(hand, TileKind.Pin4, situation);

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Fact]
    public void 子ツモ_完全一致_IsSuccessがtrue()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation
        {
            IsTsumo = true,
            PlayerWind = Wind.South,
            RoundWind = Wind.East,
        };
        var agariInfo = BuildExpected(hand, TileKind.Pin4, situation);

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Fact]
    public void 符不一致_IsSuccessがfalse()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var expected = BuildExpected(hand, TileKind.Pin4, situation);
        var agariInfo = expected with { Fu = expected.Fu + 10 };

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public void 翻不一致_IsSuccessがfalse()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var expected = BuildExpected(hand, TileKind.Pin4, situation);
        var agariInfo = expected with { Han = expected.Han + 1 };

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public void 点数不一致_IsSuccessがfalse()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var expected = BuildExpected(hand, TileKind.Pin4, situation);
        var agariInfo = expected with { TotalScore = expected.TotalScore + 1000 };

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public void 役不一致_IsSuccessがfalse()
    {
        // Arrange
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var expected = BuildExpected(hand, TileKind.Pin4, situation);
        var differentYakuList = new YakuList([Yaku.Riichi]);
        var agariInfo = expected with { YakuList = differentYakuList };

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public void 満貫以上のとき符不一致は無視される()
    {
        // Arrange: ManganType が None 以外なら Fu が違っても IsSuccess を落とさない
        var hand = new TileKindList(man: "123456", pin: "234", sou: "23444");
        var situation = new WinSituation { PlayerWind = Wind.South, RoundWind = Wind.East };
        var expected = BuildExpected(hand, TileKind.Pin4, situation, ManganType.Mangan);
        var agariInfo = expected with { Fu = expected.Fu + 999 };

        // Act
        var actual = calcValidateService_.Validate(agariInfo);

        // Assert: 他の値は一致しているので、Fu差分はスキップされ true になる
        Assert.True(actual.IsSuccess);
    }
}
