using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_IsSuufonrendaTests
{
    // 東: Kind 27 (Tile.Id 108..111)
    private static readonly Tile[] TON_TILES = [new(108), new(109), new(110), new(111)];
    // 南: Kind 28 (Tile.Id 112..115)
    private static readonly Tile[] NAN_TILES = [new(112), new(113), new(114), new(115)];

    private static Round PlaceRiver(Round round, IReadOnlyList<Tile?> tilesForEach)
    {
        var riverArray = round.RiverArray;
        for (var i = 0; i < 4; i++)
        {
            if (tilesForEach[i] is { } tile)
            {
                riverArray = riverArray.AddTile(new PlayerIndex(i), tile);
            }
        }
        return round with { RiverArray = riverArray };
    }

    [Fact]
    public void 全員同一東_副露なし_true()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        round = PlaceRiver(round, [TON_TILES[0], TON_TILES[1], TON_TILES[2], TON_TILES[3]]);

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 三人東一人南_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        round = PlaceRiver(round, [TON_TILES[0], TON_TILES[1], TON_TILES[2], NAN_TILES[0]]);

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 副露あり_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        round = PlaceRiver(round, [TON_TILES[0], TON_TILES[1], TON_TILES[2], TON_TILES[3]]);
        // 副露を強制的に追加 (四風連打は副露発生で成立しない)
        var pon = new Call(
            CallType.Pon,
            [TON_TILES[0], TON_TILES[1], TON_TILES[2]],
            new PlayerIndex(0),
            TON_TILES[0]
        );
        round = round with { CallListArray = round.CallListArray.AddCall(new PlayerIndex(1), pon) };

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 風牌以外の同一牌_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        // 白: Kind 31 (Tile.Id 124..127)
        round = PlaceRiver(round, [new Tile(124), new Tile(125), new Tile(126), new Tile(127)]);

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 一人の河が二枚_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        round = PlaceRiver(round, [TON_TILES[0], TON_TILES[1], TON_TILES[2], TON_TILES[3]]);
        // 親の河を2枚目にする
        round = round with { RiverArray = round.RiverArray.AddTile(new PlayerIndex(0), NAN_TILES[0]) };

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 一人打牌前_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        // 3人分だけ配置 (4人目未打牌)
        round = PlaceRiver(round, [TON_TILES[0], TON_TILES[1], TON_TILES[2], null]);

        // Act
        var result = round.IsSuufonrenda();

        // Assert
        Assert.False(result);
    }
}
