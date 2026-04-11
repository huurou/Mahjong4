using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.ScoreCalcValidation.Tests.Analysing.Agaris;

public class MeldParseService_ParseTests
{
    private readonly MeldParseService meldParseService_ = new(NullLogger<MeldParseService>.Instance);

    [Fact]
    public void 面子コード10607_チー四萬五萬六萬と解析される()
    {
        // Arrange
        var meldCode = 10607;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Chi(man: "456"), actual);
    }

    [Fact]
    public void 面子コード48503_チー二索三索四索と解析される()
    {
        // Arrange
        var meldCode = 48503;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Chi(sou: "234"), actual);
    }

    [Fact]
    public void 面子コード617_ポン一萬一萬一萬と解析される()
    {
        // Arrange
        var meldCode = 617;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Pon(man: "111"), actual);
    }

    [Fact]
    public void 面子コード29258_ポン二索二索二索と解析される()
    {
        // Arrange
        var meldCode = 29258;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Pon(sou: "222"), actual);
    }

    [Fact]
    public void 面子コード47210_ポン北北北と解析される()
    {
        // Arrange
        var meldCode = 47210;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Pon(honor: "ppp"), actual);
    }

    [Fact]
    public void 面子コード51241_ポン中中中と解析される()
    {
        // Arrange
        var meldCode = 51241;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Pon(honor: "ccc"), actual);
    }

    [Fact]
    public void 面子コード51249_加槓中中中中と解析される()
    {
        // Arrange
        var meldCode = 51249;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Minkan(honor: "cccc"), actual);
    }

    [Fact]
    public void 面子コード10240_暗槓二筒二筒二筒二筒と解析される()
    {
        // Arrange
        var meldCode = 10240;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Ankan(pin: "2222"), actual);
    }

    [Fact]
    public void 面子コード26624_暗槓九索九索九索九索と解析される()
    {
        // Arrange
        var meldCode = 26624;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Ankan(sou: "9999"), actual);
    }

    [Fact]
    public void 面子コード8193_大明槓九萬九萬九萬九萬と解析される()
    {
        // Arrange
        // kindValue = 8 (Man9), fromWho = 1 (下家)
        // pattern = 8 * 4 = 32, meldCode = (32 << 8) | 1 = 0x2001 = 8193
        var meldCode = 8193;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Minkan(man: "9999"), actual);
    }

    [Fact]
    public void 面子コード17411_大明槓五筒五筒五筒五筒_上家と解析される()
    {
        // Arrange
        // kindValue = 13 (Pin5), fromWho = 3 (上家)
        // pattern = 13 * 4 = 52, meldCode = (52 << 8) | 3 = 0x3403 = 13315
        var meldCode = 13315;

        // Act
        var actual = meldParseService_.Parse(meldCode);

        // Assert
        AssertCall(Call.Minkan(pin: "5555"), actual);
    }

    private static void AssertCall(Call expected, Call actual)
    {
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.TileKindList, actual.TileKindList);
    }
}
