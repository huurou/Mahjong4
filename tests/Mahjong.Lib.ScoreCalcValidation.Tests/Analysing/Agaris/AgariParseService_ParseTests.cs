using Mahjong.Lib.Calls;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;
using Mahjong.Lib.Tiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.ScoreCalcValidation.Tests.Analysing.Agaris;

public class AgariParseService_ParseTests
{
    private readonly AgariParseService agariParseService_ = new(new(NullLogger<MeldParseService>.Instance), NullLogger<AgariParseService>.Instance);

    [Fact]
    public void ツモ_鳴きなし_満貫以下_正しく解析できる()
    {
        // Arrange
        var agariNode = """
            <AGARI ba="0,2" hai="4,9,14,57,59,62,63,65,66,69,71,72,77,80" machi="71" ten="20,5200,0"
                yaku="1,1,0,1,7,1,9,1,53,0" doraHai="115" doraHaiUra="30" who="1" fromWho="1"
                sc="240,-26,240,72,250,-13,250,-13" />
            """;

        // Act
        var actual = agariParseService_.Parse(agariNode);

        // Assert
        var expected = new Agari(
            new TileKindList(man: "234", pin: "66778899", sou: "123"),
            [],
            TileKind.Pin9,
            20, 5200, ManganType.None,
            [new(0, 1), new(1, 1), new(7, 1), new(9, 1)], [],
            [TileKind.Nan], [TileKind.Man8],
            true,
            0,
            1
        );
        AssertAgari(expected, actual);
    }

    [Fact]
    public void ツモ_鳴きあり_満貫_正しく解析できる()
    {
        // Arrange
        var agariNode = """
            <AGARI ba="0,0" hai="3,5,8,50,51,61,67,70,125,126,127" m="31274" machi="67" ten="40,8000,1"
                yaku="18,1,52,3" doraHai="135" who="1" fromWho="1" sc="262,-20,329,80,271,-20,138,-40" />
            """;

        // Act
        var actual = agariParseService_.Parse(agariNode);

        // Assert
        var expected = new Agari(
            new TileKindList(man: "123", pin: "44789", honor: "hhh"),
            [Call.Pon(sou: "333")],
            TileKind.Pin8,
            40, 8000, ManganType.Mangan,
            [new(18, 1), new(52, 3)], [],
            [TileKind.Chun], [],
            true,
            0,
            1
        );
        AssertAgari(expected, actual);
    }

    [Fact]
    public void ロン_鳴きなし_満貫以下_赤1枚_正しく解析できる()
    {
        // Arrange
        var agariNode = """
            <AGARI ba="1,1" hai="38,40,44,52,57,60,83,85,91,116,118,119,125,127" machi="116" ten="40,5200,0"
                yaku="1,1,52,1,54,1,53,0" doraHai="42" doraHaiUra="64" who="1" fromWho="0"
                sc="362,-55,359,65,211,0,58,0" />
            """;

        // Act
        var actual = agariParseService_.Parse(agariNode);

        // Assert
        var expected = new Agari(
            new TileKindList(pin: "123567", sou: "345", honor: "ssshh"),
            [],
            TileKind.Sha,
            40, 5200, ManganType.None,
            [new(1, 1), new(52, 1), new(54, 1)], [],
            [TileKind.Pin2], [TileKind.Pin8],
            false,
            1,
            1
        );
        AssertAgari(expected, actual);
    }

    [Fact]
    public void ロン_鳴きあり_満貫以下_正しく解析できる()
    {
        // Arrange
        var agariNode = """
            <AGARI ba="0,2" hai="25,26,55,59,63,76,82,87" m="54375,42570" machi="82" ten="30,5800,0"
                yaku="14,1,10,1,52,1" doraHai="72" who="1" fromWho="3" sc="204,0,312,78,227,0,237,-58" />
            """;

        // Act
        var actual = agariParseService_.Parse(agariNode);

        // Assert
        var expected = new Agari(
            new TileKindList(man: "77", pin: "567", sou: "234"),
            [Call.Chi(sou: "456"), Call.Pon(honor: "ttt")],
            TileKind.Sou3,
            30, 5800, ManganType.None,
            [new(10, 1), new(14, 1), new(52, 1)], [],
            [TileKind.Sou1], [],
            false,
            0,
            1
        );
        AssertAgari(expected, actual);
    }

    [Fact]
    public void ツモ_役満_yakuman属性が解析される()
    {
        // Arrange: yakuman="47" 国士無双
        var agariNode = """
            <AGARI ba="0,0" hai="0,4,8,36,40,44,72,76,80,108,120,124,128" machi="128" ten="30,32000,5"
                yaku="" yakuman="47" doraHai="135" who="0" fromWho="0" sc="250,320,250,-110,250,-110,250,-110" />
            """;

        // Act
        var actual = agariParseService_.Parse(agariNode);

        // Assert
        Assert.Equal(ManganType.Yakuman, actual.ManganType);
        Assert.Single(actual.Yakumans);
        Assert.Equal(47, actual.Yakumans[0]);
    }

    private static void AssertAgari(Agari expected, Agari actual)
    {
        Assert.Equal(expected.Hand, actual.Hand);
        Assert.Equal(expected.CallList.Count, actual.CallList.Count);
        for (var i = 0; i < expected.CallList.Count; i++)
        {
            Assert.Equal(expected.CallList[i].Type, actual.CallList[i].Type);
            Assert.Equal(expected.CallList[i].TileKindList, actual.CallList[i].TileKindList);
        }
        Assert.Equal(expected.WinTile, actual.WinTile);
        Assert.Equal(expected.Fu, actual.Fu);
        Assert.Equal(expected.Score, actual.Score);
        Assert.Equal(expected.ManganType, actual.ManganType);
        Assert.Equal(expected.YakuInfos, actual.YakuInfos);
        Assert.Equal(expected.DoraIndicators, actual.DoraIndicators);
        Assert.Equal(expected.UradoraIndicators, actual.UradoraIndicators);
        Assert.Equal(expected.IsTsumo, actual.IsTsumo);
        Assert.Equal(expected.AkadoraCount, actual.AkadoraCount);
    }
}
