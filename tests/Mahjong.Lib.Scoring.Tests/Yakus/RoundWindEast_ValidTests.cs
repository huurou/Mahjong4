using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class RoundWindEast_ValidTests
{
    [Fact]
    public void Valid_東場で東の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_東場で副露の東の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Pon(new TileKindList(honor: "ttt"))]);
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_東場で東の明槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Minkan(new TileKindList(honor: "tttt"))]);
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_東場で東の暗槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Ankan(new TileKindList(honor: "tttt"))]);
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_東場で南の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "nnn")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_東場で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_東場で北の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ppp")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_東場で東の牌があるが刻子になっていない場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "tt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南場で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.South };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南場で南の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "nnn")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.South };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西場で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西場で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_北場で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.North };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_北場で北の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ppp")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.North };

        // Act
        var actual = RoundWindEast.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
