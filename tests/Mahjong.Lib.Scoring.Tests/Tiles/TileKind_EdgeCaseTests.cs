using Mahjong.Lib.Scoring.Tiles;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

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

    private static Exception? RecordExceptionSafe(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
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
    public void GetActualDora_範囲外の値_ArgumentOutOfRangeException発生()
    {
        // Arrange
        var tile = CreateInvalidTileKind(34);

        // Act
        var ex = RecordExceptionSafe(() => TileKind.GetActualDora(tile));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact]
    public void ToString_範囲外の値_ArgumentOutOfRangeException発生()
    {
        // Arrange
        var tile = CreateInvalidTileKind(34);

        // Act
        var ex = Record.Exception(tile.ToString);

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }
}
