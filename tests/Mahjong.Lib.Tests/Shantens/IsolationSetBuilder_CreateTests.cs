using Mahjong.Lib.Shantens;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Shantens;

public class IsolationSetBuilder_CreateTests
{
    [Fact]
    public void ReadOnlySpanから正常に作成される()
    {
        // Arrange
        var tileArray = new TileKind[] { TileKind.Man1, TileKind.Pin2, TileKind.Sou3 };

        // Act
        var isolationSet = IsolationSet.IsolationSetBuilder.Create(tileArray);

        // Assert
        Assert.Equal(3, isolationSet.Count);
        Assert.Contains(TileKind.Man1, isolationSet);
        Assert.Contains(TileKind.Pin2, isolationSet);
        Assert.Contains(TileKind.Sou3, isolationSet);
    }

    [Fact]
    public void 空のSpan_空のIsolationSetが作成される()
    {
        // Arrange
        var emptySpan = ReadOnlySpan<TileKind>.Empty;

        // Act
        var isolationSet = IsolationSet.IsolationSetBuilder.Create(emptySpan);

        // Assert
        Assert.Equal(0, isolationSet.Count);
        Assert.Empty(isolationSet);
    }

    [Fact]
    public void 重複するSpan_重複は除去される()
    {
        // Arrange
        var tileArray = new TileKind[] { TileKind.Man1, TileKind.Man1, TileKind.Pin2 };

        // Act
        var isolationSet = IsolationSet.IsolationSetBuilder.Create(tileArray);

        // Assert
        Assert.Equal(2, isolationSet.Count);
        Assert.Contains(TileKind.Man1, isolationSet);
        Assert.Contains(TileKind.Pin2, isolationSet);
    }

    [Fact]
    public void コレクションビルダー初期化構文_正常に動作する()
    {
        // Arrange & Act
        IsolationSet isolationSet = [TileKind.Man1, TileKind.Pin2, TileKind.Sou3];

        // Assert
        Assert.Equal(3, isolationSet.Count);
        Assert.Contains(TileKind.Man1, isolationSet);
        Assert.Contains(TileKind.Pin2, isolationSet);
        Assert.Contains(TileKind.Sou3, isolationSet);
    }
}
