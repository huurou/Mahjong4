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
        var pon = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            new PlayerIndex(0),
            new Tile(87)
        );
        var p1 = new PlayerIndex(1);
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
        // P0 打牌 Tile(84) (kind 21)。 P1 の手牌に kind 21 の残り3枚 (Tile(85,86,87)) を作る。
        // P1 の配牌には Tile(86) のみなので、テスト用に Tile(85), Tile(87) を手牌に注入する。
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = round with
        {
            HandArray = round.HandArray.AddTile(new PlayerIndex(0), new Tile(84))
        };
        round = round.Dahai(new Tile(84));
        var caller = new PlayerIndex(1);
        var handTiles = ImmutableList.Create(new Tile(85), new Tile(86), new Tile(87));
        round = round with
        {
            HandArray = round.HandArray.AddTile(caller, new Tile(85)).AddTile(caller, new Tile(87))
        };
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
        var pon = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            new PlayerIndex(0),
            new Tile(87)
        );
        var p1 = new PlayerIndex(1);
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
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo().Ankan(new Tile(135));
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
        // Arrange: P1 に pon を 2つ用意し、2回加槓できる状態を作る
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var p1 = new PlayerIndex(1);
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
        // P1 は加槓に使う Tile(86) は持つが、Tile(90) は持たないので手牌に注入する
        round = round with
        {
            CallListArray = callListArray,
            Turn = p1,
            HandArray = round.HandArray.AddTile(p1, new Tile(90))
        };
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
