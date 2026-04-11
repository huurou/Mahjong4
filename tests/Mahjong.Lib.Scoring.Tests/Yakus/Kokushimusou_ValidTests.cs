using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Kokushimusou_ValidTests
{
    [Fact]
    public void Valid_老頭牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "tnsphrc");

        // Act
        var actual = Kokushimusou.Valid(tileKindList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");

        // Act
        var actual = Kokushimusou.Valid(tileKindList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_三元牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "tnsphrcc");

        // Act
        var actual = Kokushimusou.Valid(tileKindList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_全ての么九牌が揃っていない_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "tnsphr");

        // Act
        var actual = Kokushimusou.Valid(tileKindList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_中張牌が含まれる_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "159", pin: "19", sou: "19", honor: "tnsphrc");

        // Act
        var actual = Kokushimusou.Valid(tileKindList);

        // Assert
        Assert.False(actual);
    }
}
