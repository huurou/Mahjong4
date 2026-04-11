using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Shantens;

public class IsolationSet_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空のIsolationSetが作成される()
    {
        // Arrange & Act
        var isolationSet = new IsolationSet();

        // Assert
        Assert.Equal(0, isolationSet.Count);
        Assert.Empty(isolationSet);
    }

    [Fact]
    public void 空のコレクション_空のIsolationSetが作成される()
    {
        // Arrange
        var emptyTiles = new List<TileKind>();

        // Act
        var isolationSet = new IsolationSet(emptyTiles);

        // Assert
        Assert.Equal(0, isolationSet.Count);
        Assert.Empty(isolationSet);
    }

    [Fact]
    public void 有効なタイル_正常に作成される()
    {
        // Arrange
        var tiles = new List<TileKind> { TileKind.Man1, TileKind.Man5, TileKind.Man9 };

        // Act
        var isolationSet = new IsolationSet(tiles);

        // Assert
        Assert.Equal(3, isolationSet.Count);
        Assert.Contains(TileKind.Man1, isolationSet);
        Assert.Contains(TileKind.Man5, isolationSet);
        Assert.Contains(TileKind.Man9, isolationSet);
    }

    [Fact]
    public void 重複するタイル_重複は除去される()
    {
        // Arrange
        var tiles = new List<TileKind> { TileKind.Man1, TileKind.Man1, TileKind.Man5 };

        // Act
        var isolationSet = new IsolationSet(tiles);

        // Assert
        Assert.Equal(2, isolationSet.Count);
        Assert.Contains(TileKind.Man1, isolationSet);
        Assert.Contains(TileKind.Man5, isolationSet);
    }

    [Fact]
    public void すべての牌種_IsolationSetに追加できる()
    {
        // Arrange
        var allTileKinds = new List<TileKind>
        {
            // 萬子
            TileKind.Man1, TileKind.Man2, TileKind.Man3, TileKind.Man4, TileKind.Man5,
            TileKind.Man6, TileKind.Man7, TileKind.Man8, TileKind.Man9,
            // 筒子
            TileKind.Pin1, TileKind.Pin2, TileKind.Pin3, TileKind.Pin4, TileKind.Pin5,
            TileKind.Pin6, TileKind.Pin7, TileKind.Pin8, TileKind.Pin9,
            // 索子
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3, TileKind.Sou4, TileKind.Sou5,
            TileKind.Sou6, TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            // 字牌
            TileKind.Ton, TileKind.Nan, TileKind.Sha, TileKind.Pei,
            TileKind.Haku, TileKind.Hatsu, TileKind.Chun
        };

        // Act
        var isolationSet = new IsolationSet(allTileKinds);

        // Assert
        Assert.Equal(34, isolationSet.Count);
        foreach (var tileKind in allTileKinds)
        {
            Assert.Contains(tileKind, isolationSet);
        }
    }
}
