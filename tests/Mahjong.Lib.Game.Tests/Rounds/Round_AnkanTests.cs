using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_AnkanTests
{
    [Fact]
    public void Ankan_手牌から四枚が除かれCallListに追加される()
    {
        // Arrange
        // 親 P0 の手牌に kind 33 の4枚 + 任意10枚 (計14枚)
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(132), new Tile(133), new Tile(134), new Tile(135),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13),
        ]);
        var tile = new Tile(135);

        // Act
        var result = round.Ankan(tile);

        // Assert
        Assert.Equal(10, result.HandArray[round.Turn].Count());
        Assert.Single(result.CallListArray[round.Turn]);
        var call = result.CallListArray[round.Turn].First();
        Assert.Equal(CallType.Ankan, call.Type);
        Assert.Equal(4, call.Tiles.Count);
    }

    [Fact]
    public void Ankan_同種4枚未満_InvalidOperationExceptionが発生する()
    {
        // Arrange
        // 手牌に kind 21 は Tile(87) 1枚のみ
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(87),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16), new Tile(17),
            new Tile(20),
        ]);
        var tile = new Tile(87);

        // Act
        var ex = Record.Exception(() => round.Ankan(tile));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Ankan_ツモ山が空_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(132), new Tile(133), new Tile(134), new Tile(135),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13),
        ]);
        // ツモ山を空にする
        var wall = round.Wall;
        while (wall.LiveRemaining > 0)
        {
            wall = wall.Draw(out _);
        }
        round = round with { Wall = wall };
        var tile = new Tile(135);

        // Act
        var ex = Record.Exception(() => round.Ankan(tile));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }
}
