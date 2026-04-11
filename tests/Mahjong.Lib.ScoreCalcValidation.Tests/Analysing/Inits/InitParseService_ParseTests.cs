using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Inits;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.ScoreCalcValidation.Tests.Analysing.Inits;

public class InitParseService_ParseTests
{
    private readonly InitParseService initParseService_ = new(NullLogger<InitParseService>.Instance);

    [Fact]
    public void 東一局_親番0_正しく解析できる()
    {
        // Arrange
        var initNode = """
            <INIT seed="0,0,0,3,5,115" ten="250,250,250,250" oya="0"
                hai0="19,88,50,87,105,133,44,51,42,32,134,54,67"
                hai1="62,69,49,65,66,14,0,132,104,123,57,99,125"
                hai2="7,16,12,40,108,47,121,86,78,60,15,21,56"
                hai3="126,13,127,23,33,61,100,37,64,103,11,122,27" />
            """;

        // Act
        var actual = initParseService_.Parse(initNode);

        // Assert
        Assert.Equal(new Init(0, 0, Wind.East, 0), actual);
    }

    [Fact]
    public void 東二局1本場_親番1_正しく解析できる()
    {
        // Arrange
        var initNode = """
            <INIT seed="1,1,0,1,5,7" ten="204,390,227,179" oya="1"
                hai0="32,103,47,53,110,120,91,111,81,6,97,37,12"
                hai1="80,67,36,98,58,118,129,19,131,125,124,49,113"
                hai2="68,135,11,122,99,10,54,105,126,83,14,66,27"
                hai3="121,128,116,4,107,73,17,77,96,39,35,92,1" />
            """;

        // Act
        var actual = initParseService_.Parse(initNode);

        // Assert
        Assert.Equal(new Init(1, 1, Wind.East, 1), actual);
    }

    [Fact]
    public void 南一局_親番0_正しく解析できる()
    {
        // Arrange
        var initNode = """
            <INIT seed="4,0,0,5,0,43" ten="242,409,251,98" oya="0"
                hai0="112,90,89,19,63,30,12,13,125,97,49,80,31"
                hai1="129,110,111,113,39,107,85,104,132,119,18,98,54"
                hai2="69,93,124,59,3,0,130,55,94,121,41,65,86"
                hai3="126,79,34,38,56,75,100,16,6,87,108,25,70" />
            """;

        // Act
        var actual = initParseService_.Parse(initNode);

        // Assert
        Assert.Equal(new Init(4, 0, Wind.South, 0), actual);
    }
}
