using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameRules_IsRedDoraTests
{
    [Fact]
    public void Defaultで赤五萬_trueを返す()
    {
        // Arrange
        var rules = new GameRules();

        // Act
        var result = rules.IsRedDora(new Tile(16));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Defaultで赤五筒_trueを返す()
    {
        // Arrange
        var rules = new GameRules();

        // Act
        var result = rules.IsRedDora(new Tile(52));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Defaultで赤五索_trueを返す()
    {
        // Arrange
        var rules = new GameRules();

        // Act
        var result = rules.IsRedDora(new Tile(88));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Defaultで赤ドラ以外_falseを返す()
    {
        // Arrange
        var rules = new GameRules();

        // Act
        var result = rules.IsRedDora(new Tile(0));

        // Assert
        Assert.False(result);
    }
}
