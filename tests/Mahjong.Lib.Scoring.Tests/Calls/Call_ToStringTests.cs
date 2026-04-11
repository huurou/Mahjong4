using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class Call_ToStringTests
{
    [Fact]
    public void チー_種類と構成牌の文字列を返す()
    {
        // Arrange
        var call = Call.Chi(new TileKindList(man: "123"));

        // Act
        var result = call.ToString();

        // Assert
        Assert.Equal("チー-一二三", result);
    }

    [Fact]
    public void ポン_種類と構成牌の文字列を返す()
    {
        // Arrange
        var call = Call.Pon(new TileKindList(man: "111"));

        // Act
        var result = call.ToString();

        // Assert
        Assert.Equal("ポン-一一一", result);
    }

    [Fact]
    public void 暗槓_種類と構成牌の文字列を返す()
    {
        // Arrange
        var call = Call.Ankan(new TileKindList(man: "1111"));

        // Act
        var result = call.ToString();

        // Assert
        Assert.Equal("暗槓-一一一一", result);
    }

    [Fact]
    public void 明槓_種類と構成牌の文字列を返す()
    {
        // Arrange
        var call = Call.Minkan(new TileKindList(man: "1111"));

        // Act
        var result = call.ToString();

        // Assert
        Assert.Equal("明槓-一一一一", result);
    }

    [Fact]
    public void 抜き_種類と構成牌の文字列を返す()
    {
        // Arrange
        var call = Call.Nuki(new TileKindList(man: "1"));

        // Act
        var result = call.ToString();

        // Assert
        Assert.Equal("抜き-一", result);
    }
}
