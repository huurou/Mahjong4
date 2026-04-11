using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Rounds;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests.Analysing.Rounds;

public class RoundDataExtractService_ExtractTests
{
    private readonly RoundDataExtractService roundDataExtractService_ = new(NullLogger<RoundDataExtractService>.Instance);

    [Fact]
    public void 改行のある牌譜_局データを正しく抽出できる()
    {
        // Arrange
        var paifuXml = File.ReadAllText(Path.Combine("TestData", "PaifuWithLineBreaks.xml"));

        // Act
        var actual = roundDataExtractService_.Extract(new("", paifuXml));

        // Assert
        Assert.Equal(11, actual.Count);
        Assert.Equal(11, actual.Aggregate(0, (a, x) => a + x.AgariTags.Count));
    }

    [Fact]
    public void 改行のない牌譜_局データを正しく抽出できる()
    {
        // Arrange
        var paifuXml = File.ReadAllText(Path.Combine("TestData", "PaifuWithoutLineBreaks.xml"));

        // Act
        var actual = roundDataExtractService_.Extract(new("", paifuXml));

        // Assert
        Assert.Equal(12, actual.Count);
        Assert.Equal(12, actual.Aggregate(0, (a, x) => a + x.AgariTags.Count));
    }

    [Fact]
    public void 改行のない牌譜_ダブロン_局データを正しく抽出できる()
    {
        // Arrange
        var paifuXml = File.ReadAllText(Path.Combine("TestData", "PaifuWithoutLineBreaksDoubleRon.xml"));

        // Act
        var actual = roundDataExtractService_.Extract(new("", paifuXml));

        // Assert
        Assert.Equal(13, actual.Count);
        Assert.Equal(14, actual.Aggregate(0, (a, x) => a + x.AgariTags.Count));
    }

    [Fact]
    public void 流局タグで局が終わる場合_その局は破棄される()
    {
        // Arrange
        // INIT→AGARI→INIT→RYUUKYOKU→INIT→AGARI
        // → 1局目と3局目のみ残り、2局目(流局) は破棄される
        var paifuXml =
            "<INIT seed=\"0,0,0,0,0,0\" oya=\"0\" />" +
            "<AGARI ba=\"0,0\" hai=\"\" machi=\"0\" ten=\"20,1000,0\" yaku=\"\" doraHai=\"\" who=\"0\" fromWho=\"0\" />" +
            "<INIT seed=\"1,0,0,0,0,0\" oya=\"1\" />" +
            "<RYUUKYOKU ba=\"0,0\" />" +
            "<INIT seed=\"2,0,0,0,0,0\" oya=\"2\" />" +
            "<AGARI ba=\"0,0\" hai=\"\" machi=\"0\" ten=\"20,1000,0\" yaku=\"\" doraHai=\"\" who=\"0\" fromWho=\"0\" />";

        // Act
        var actual = roundDataExtractService_.Extract(new("", paifuXml));

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal(2, actual.Aggregate(0, (a, x) => a + x.AgariTags.Count));
    }

    [Fact]
    public void AGARIタグがINITタグの前にある場合_例外が発生する()
    {
        // Arrange
        var paifuXml = "<AGARI ba=\"0,0\" hai=\"\" machi=\"0\" ten=\"20,1000,0\" yaku=\"\" doraHai=\"\" who=\"0\" fromWho=\"0\" />";

        // Act
        var exception = Record.Exception(() => roundDataExtractService_.Extract(new("", paifuXml)));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 局外で流局タグがある場合_破棄ログは出ない()
    {
        // Arrange
        // RYUUKYOKU→INIT→AGARI
        // 先頭の RYUUKYOKU は currentRound が null のまま、ログは出ずに無視される
        var paifuXml =
            "<RYUUKYOKU ba=\"0,0\" />" +
            "<INIT seed=\"0,0,0,0,0,0\" oya=\"0\" />" +
            "<AGARI ba=\"0,0\" hai=\"\" machi=\"0\" ten=\"20,1000,0\" yaku=\"\" doraHai=\"\" who=\"0\" fromWho=\"0\" />";

        // Act
        var actual = roundDataExtractService_.Extract(new("", paifuXml));

        // Assert
        Assert.Single(actual);
        Assert.Single(actual[0].AgariTags);
    }
}
