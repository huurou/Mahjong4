using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_KakanTests
{
    [Fact]
    public void Kakan_既存のポンが加槓に差し替わり手牌から加槓牌が除かれる()
    {
        // Arrange
        // P1 に kind 21 のポン (Tile 84,85,87) が既にあり、手牌の Tile(86) を加槓する
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var p1 = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(86),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16), new Tile(17),
        ]);
        var pon = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            new PlayerIndex(0),
            new Tile(87)
        );
        var callListArray = round.CallListArray.AddCall(p1, pon);
        round = round with { CallListArray = callListArray, Turn = p1 };

        // Act
        var result = round.Kakan(new Tile(86));

        // Assert
        Assert.DoesNotContain(new Tile(86), result.HandArray[p1]);
        Assert.Single(result.CallListArray[p1]);
        var kakan = result.CallListArray[p1].First();
        Assert.Equal(CallType.Kakan, kakan.Type);
        Assert.Equal(4, kakan.Tiles.Count);
        Assert.Contains(new Tile(86), kakan.Tiles);
    }

    [Fact]
    public void Kakan_複数副露がある場合_元のポンの位置で加槓に置き換わる()
    {
        // Arrange: P1 にポン2つ (kind 21, kind 22)。最初のポン (kind 21) を加槓する
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var p1 = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(86),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        var pon1 = new Call(CallType.Pon, [new Tile(84), new Tile(85), new Tile(87)], new PlayerIndex(0), new Tile(87));
        var pon2 = new Call(CallType.Pon, [new Tile(88), new Tile(89), new Tile(91)], new PlayerIndex(0), new Tile(91));
        var callListArray = round.CallListArray.AddCall(p1, pon1).AddCall(p1, pon2);
        round = round with { CallListArray = callListArray, Turn = p1 };

        // Act: 最初のポン (kind 21) を加槓
        var result = round.Kakan(new Tile(86));

        // Assert: index 0 が加槓、index 1 がポンのまま
        Assert.Equal(2, result.CallListArray[p1].Count);
        var first = result.CallListArray[p1].First();
        var second = result.CallListArray[p1].Skip(1).First();
        Assert.Equal(CallType.Kakan, first.Type);
        Assert.Equal(CallType.Pon, second.Type);
    }

    [Fact]
    public void Kakan_対応ポンが無い_InvalidOperationExceptionが発生する()
    {
        // Arrange
        // P0 の手牌に Tile(87) (kind 21) はあるが、対応するポンは無い
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(87),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16), new Tile(17),
            new Tile(20),
        ]);

        // Act
        var ex = Record.Exception(() => round.Kakan(new Tile(87)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }
}
