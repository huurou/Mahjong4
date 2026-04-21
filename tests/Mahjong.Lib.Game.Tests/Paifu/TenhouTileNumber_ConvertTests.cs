using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouTileNumber_ConvertTests
{
    private static readonly GameRules AkaRules = new();
    private static readonly GameRules NoAkaRules = new() { RedDoraTiles = [] };

    [Theory]
    [InlineData(0, 11)] // 1m
    [InlineData(4, 12)] // 2m
    [InlineData(32, 19)] // 9m
    public void 萬子_11から19を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), NoAkaRules);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(36, 21)] // 1p
    [InlineData(52, 25)] // 5p (赤なしルール)
    [InlineData(68, 29)] // 9p
    public void 筒子_21から29を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), NoAkaRules);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(72, 31)] // 1s
    [InlineData(88, 35)] // 5s (赤なしルール)
    [InlineData(104, 39)] // 9s
    public void 索子_31から39を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), NoAkaRules);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(108, 41)] // 東
    [InlineData(112, 42)] // 南
    [InlineData(116, 43)] // 西
    [InlineData(120, 44)] // 北
    [InlineData(124, 45)] // 白
    [InlineData(128, 46)] // 發
    [InlineData(132, 47)] // 中
    public void 字牌_41から47を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), NoAkaRules);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(16, 51)] // 赤5m
    [InlineData(52, 52)] // 赤5p
    [InlineData(88, 53)] // 赤5s
    public void 赤ドラ有効ルール_51から53を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), AkaRules);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(16, 15)] // 赤5m のはずが赤なしルールなので通常 5m
    [InlineData(52, 25)] // 赤5p のはずが通常 5p
    [InlineData(88, 35)] // 赤5s のはずが通常 5s
    public void 赤ドラ無効ルール_通常牌番号を返す(int tileId, int expected)
    {
        var result = TenhouTileNumber.Convert(new Tile(tileId), NoAkaRules);

        Assert.Equal(expected, result);
    }
}
