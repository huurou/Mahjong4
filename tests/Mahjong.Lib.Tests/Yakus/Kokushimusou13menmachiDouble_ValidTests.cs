using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Kokushimusou13menmachiDouble_ValidTests
{

    [Fact]
    public void Valid_ダブル役満有効_国士無双十三面待ちの条件を満たす_成立する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "tnsphrc");
        var winTile = TileKind.Man1;
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = Kokushimusou13menmachiDouble.Valid(tileKindList, winTile, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ダブル役満無効_国士無双十三面待ちの条件を満たす_成立しない()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "119", pin: "19", sou: "19", honor: "tnsphrc");
        var winTile = TileKind.Man1;
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = Kokushimusou13menmachiDouble.Valid(tileKindList, winTile, gameRules);

        // Assert
        Assert.False(actual);
    }
}
