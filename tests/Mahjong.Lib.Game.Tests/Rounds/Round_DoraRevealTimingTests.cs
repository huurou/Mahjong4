using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_DoraRevealTimingTests
{
    [Fact]
    public void Ankan_即座にドラが1枚めくられPendingDoraRevealはfalse()
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
        var initialDora = round.Wall.DoraRevealedCount;

        // Act
        var result = round.Ankan(new Tile(135));

        // Assert
        Assert.Equal(initialDora + 1, result.Wall.DoraRevealedCount);
        Assert.False(result.PendingDoraReveal);
    }

    [Fact]
    public void Kakan_ドラはめくられずPendingDoraRevealがtrueになる()
    {
        // Arrange
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
        var initialDora = round.Wall.DoraRevealedCount;

        // Act
        var result = round.Kakan(new Tile(86));

        // Assert
        Assert.Equal(initialDora, result.Wall.DoraRevealedCount);
        Assert.True(result.PendingDoraReveal);
    }

    [Fact]
    public void Daiminkan_ドラはめくられずPendingDoraRevealがtrueになる()
    {
        // Arrange
        // P0 が Tile(84) (kind 21) を打牌 → P1 が kind 21 の残り3枚 (Tile 85,86,87) で大明槓
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(84),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(16), new Tile(17), new Tile(18), new Tile(19),
            new Tile(28), new Tile(29), new Tile(30), new Tile(31),
            new Tile(40),
        ]);
        round = round.Dahai(new Tile(84));
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(85), new Tile(86), new Tile(87),
            new Tile(8), new Tile(9), new Tile(10), new Tile(11),
            new Tile(20), new Tile(21), new Tile(22), new Tile(23),
            new Tile(32), new Tile(33),
        ]);
        var handTiles = ImmutableList.Create(new Tile(85), new Tile(86), new Tile(87));
        var initialDora = round.Wall.DoraRevealedCount;

        // Act
        var result = round.Daiminkan(caller, handTiles);

        // Assert
        Assert.Equal(initialDora, result.Wall.DoraRevealedCount);
        Assert.True(result.PendingDoraReveal);
    }

    [Fact]
    public void RinshanTsumo_PendingDoraRevealがtrueならドラを1枚めくりフラグをクリア()
    {
        // Arrange: Kakan で保留状態を作る
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
        round = (round with { CallListArray = callListArray, Turn = p1 }).Kakan(new Tile(86));
        var doraBeforeRinshan = round.Wall.DoraRevealedCount;

        // Act
        var result = round.RinshanTsumo();

        // Assert
        Assert.Equal(doraBeforeRinshan + 1, result.Wall.DoraRevealedCount);
        Assert.False(result.PendingDoraReveal);
    }

    [Fact]
    public void RinshanTsumo_PendingDoraRevealがfalseならドラはめくられない()
    {
        // Arrange: Ankan は即めくり後にフラグfalse
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = RoundTestHelper.InjectHand(round, new PlayerIndex(0),
        [
            new Tile(132), new Tile(133), new Tile(134), new Tile(135),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13),
        ]);
        round = round.Ankan(new Tile(135));
        var doraBeforeRinshan = round.Wall.DoraRevealedCount;

        // Act
        var result = round.RinshanTsumo();

        // Assert
        Assert.Equal(doraBeforeRinshan, result.Wall.DoraRevealedCount);
        Assert.False(result.PendingDoraReveal);
    }

    [Fact]
    public void 連続加槓_各嶺上ツモで1枚ずつめくられる()
    {
        // Arrange: P1 に kind 21 と kind 22 のポンを用意、手牌に加槓牌 Tile(86), Tile(90) を含める
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var p1 = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(86), new Tile(90),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        var pon1 = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            new PlayerIndex(0),
            new Tile(87)
        );
        var pon2 = new Call(
            CallType.Pon,
            [new Tile(88), new Tile(89), new Tile(91)],
            new PlayerIndex(0),
            new Tile(91)
        );
        var callListArray = round.CallListArray.AddCall(p1, pon1).AddCall(p1, pon2);
        round = round with { CallListArray = callListArray, Turn = p1 };
        var initialDora = round.Wall.DoraRevealedCount;

        // Act: 加槓1 → 嶺上ツモ → 加槓2 → 嶺上ツモ
        var afterKakan1 = round.Kakan(new Tile(86));
        var afterRinshan1 = afterKakan1.RinshanTsumo();
        var afterKakan2 = afterRinshan1.Kakan(new Tile(90));
        var afterRinshan2 = afterKakan2.RinshanTsumo();

        // Assert
        Assert.Equal(initialDora, afterKakan1.Wall.DoraRevealedCount);
        Assert.True(afterKakan1.PendingDoraReveal);
        Assert.Equal(initialDora + 1, afterRinshan1.Wall.DoraRevealedCount);
        Assert.False(afterRinshan1.PendingDoraReveal);
        Assert.Equal(initialDora + 1, afterKakan2.Wall.DoraRevealedCount);
        Assert.True(afterKakan2.PendingDoraReveal);
        Assert.Equal(initialDora + 2, afterRinshan2.Wall.DoraRevealedCount);
        Assert.False(afterRinshan2.PendingDoraReveal);
    }
}
