using System.Reflection;
using System.Runtime.CompilerServices;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_EdgeCaseTests
{
    private static TileKind CreateInvalidTileKind(int value)
    {
        var tile = (TileKind)RuntimeHelpers.GetUninitializedObject(typeof(TileKind));
        typeof(TileKind)
            .GetField("<Value>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(tile, value);
        return tile;
    }

    [Fact]
    public void IsNumber_負の値_falseを返す()
    {
        // Arrange
        var tile = CreateInvalidTileKind(-1);

        // Act & Assert
        Assert.False(tile.IsNumber);
    }

    [Fact]
    public void IsMan_負の値_falseを返す()
    {
        // Arrange
        var tile = CreateInvalidTileKind(-1);

        // Act & Assert
        Assert.False(tile.IsMan);
    }

    [Fact]
    public void GetActualDora_範囲外の値_例外が発生する()
    {
        // Arrange
        var tile = CreateInvalidTileKind(34);
        var thrown = false;

        // Act
        try
        {
            TileKind.GetActualDora(tile);
        }
        catch (ArgumentOutOfRangeException)
        {
            thrown = true;
        }

        // Assert
        Assert.True(thrown);
    }

    [Fact]
    public void ToString_範囲外の値_例外が発生する()
    {
        // Arrange
        var tile = CreateInvalidTileKind(34);
        var thrown = false;

        // Act
        try
        {
            tile.ToString();
        }
        catch (ArgumentOutOfRangeException)
        {
            thrown = true;
        }

        // Assert
        Assert.True(thrown);
    }
}
