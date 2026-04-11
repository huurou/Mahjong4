using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Kokushimusou13menmachi_ValidTests
{
    [Fact]
    public void Valid_老頭牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "tnsphrc");
        var winTile = TileKind.Man1;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_風牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "ttnsphrc");
        var winTile = TileKind.Ton;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_三元牌が重複_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "tnsphrcc");
        var winTile = TileKind.Chun;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_和了牌が一枚しかない_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "19", pin: "19", sou: "19", honor: "tnsphrc");
        var winTile = TileKind.Man1;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_国士無双ではない_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "ttnsph");
        var winTile = TileKind.Man1;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_中張牌が含まれる_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "159", pin: "19", sou: "19", honor: "tnsphrc");
        var winTile = TileKind.Man5;

        // Act
        var actual = Kokushimusou13menmachi.Valid(tileKindList, winTile);

        // Assert
        Assert.False(actual);
    }
}
