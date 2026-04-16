using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Candidates;

public class ResponseCandidate_EqualsTests
{
    [Fact]
    public void 同じOkCandidate_等価()
    {
        // Arrange
        var a = new OkCandidate();
        var b = new OkCandidate();

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void 同じAnkanCandidate_同一インスタンス共有_等価()
    {
        // Arrange (同一コレクションインスタンスを共有)
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var a = new AnkanCandidate(tiles);
        var b = new AnkanCandidate(tiles);

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void 同じChiCandidate_同一インスタンス共有_等価()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(4));
        var a = new ChiCandidate(tiles);
        var b = new ChiCandidate(tiles);

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void 同じChiCandidate_別インスタンス同要素_構造等価により等価()
    {
        // Arrange (同じ要素だが別コレクションインスタンス)
        var a = new ChiCandidate([new Tile(0), new Tile(4)]);
        var b = new ChiCandidate([new Tile(0), new Tile(4)]);

        // Assert (カスタム Equals で構造比較)
        Assert.Equal(a, b);
    }

    [Fact]
    public void 同じAnkanCandidate_別インスタンス同要素_構造等価により等価()
    {
        // Arrange (同じ要素だが別コレクションインスタンス)
        var a = new AnkanCandidate([new Tile(0), new Tile(1), new Tile(2), new Tile(3)]);
        var b = new AnkanCandidate([new Tile(0), new Tile(1), new Tile(2), new Tile(3)]);

        // Assert (カスタム Equals で構造比較)
        Assert.Equal(a, b);
    }

    [Fact]
    public void 同じDahaiCandidate_別インスタンス同要素_構造等価により等価()
    {
        // Arrange
        var a = new DahaiCandidate([new DahaiOption(new Tile(0), true)]);
        var b = new DahaiCandidate([new DahaiOption(new Tile(0), true)]);

        // Assert (DahaiOptionList のカスタム Equals で構造比較)
        Assert.Equal(a, b);
    }

    [Fact]
    public void 異なる型のCandidate_非等価()
    {
        // Arrange
        ResponseCandidate a = new OkCandidate();
        ResponseCandidate b = new TsumoAgariCandidate();

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void 同じKyuushuKyuuhaiCandidate_等価()
    {
        // Arrange
        var a = new KyuushuKyuuhaiCandidate();
        var b = new KyuushuKyuuhaiCandidate();

        // Assert
        Assert.Equal(a, b);
    }
}
