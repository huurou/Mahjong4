using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_AnkanTests
{
    [Fact]
    public void Ankan_手牌から四枚が除かれCallListに追加される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        // 親の手牌に含まれる同種4枚 (yama[135,134,133,132]=kind 33)
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
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        // kind 21 は Tile(87) 1枚のみ
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
