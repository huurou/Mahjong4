using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_SettleWinTests
{
    // 点数移動のみを検証するテスト群で winTile の値にはこだわらないためのダミー牌
    private static Tile DummyWinTile { get; } = new(0);

    private static ScoreResult Result(params (int index, int delta)[] deltas)
    {
        var array = new PointArray(new Point(0));
        foreach (var (index, delta) in deltas)
        {
            array = delta >= 0
                ? array.AddPoint(new PlayerIndex(index), delta)
                : array.SubtractPoint(new PlayerIndex(index), -delta);
        }
        return new ScoreResult(0, 0, array, [], IsMenzen: false);
    }

    private static Round CreateBaseRound()
    {
        return RoundTestHelper.CreateRound().Haipai()
             with
        { PointArray = new PointArray(new Point(25000)) };
    }

    [Fact]
    public void ScoreResultのPointDeltasがPointArrayに適用される()
    {
        // Arrange
        var round = CreateBaseRound();
        var dealer = new PlayerIndex(0);
        var scoreResults = ImmutableArray.Create(Result((0, 12000), (1, -4000), (2, -4000), (3, -4000)));

        // Act
        var (settled, _) = round.SettleWin([dealer], dealer, WinType.Tsumo, DummyWinTile, scoreResults);

        // Assert
        Assert.Equal(25000 + 12000, settled.PointArray[dealer].Value);
        Assert.Equal(25000 - 4000, settled.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000, settled.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000, settled.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 本場1本_親ロンで放銃者から和了者へ300点追加移動()
    {
        // Arrange
        var round = CreateBaseRound() with { Honba = new Honba(1) };
        var dealer = new PlayerIndex(0);
        var loser = new PlayerIndex(2);
        var scoreResults = ImmutableArray.Create(Result((0, 12000), (2, -12000)));

        // Act
        var (settled, _) = round.SettleWin([dealer], loser, WinType.Ron, DummyWinTile, scoreResults);

        // Assert
        Assert.Equal(25000 + 12000 + 300, settled.PointArray[dealer].Value);
        Assert.Equal(25000 - 12000 - 300, settled.PointArray[loser].Value);
    }

    [Fact]
    public void 本場2本_親ツモで各他家から100点追加移動()
    {
        // Arrange
        var round = CreateBaseRound() with { Honba = new Honba(2) };
        var dealer = new PlayerIndex(0);
        var scoreResults = ImmutableArray.Create(Result((0, 12000), (1, -4000), (2, -4000), (3, -4000)));

        // Act
        var (settled, _) = round.SettleWin([dealer], dealer, WinType.Tsumo, DummyWinTile, scoreResults);

        // Assert
        Assert.Equal(25000 + 12000 + 600, settled.PointArray[dealer].Value);
        Assert.Equal(25000 - 4000 - 200, settled.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000 - 200, settled.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000 - 200, settled.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 供託1本_和了者に1000点加算され供託は0になる()
    {
        // Arrange
        var round = CreateBaseRound() with { KyoutakuRiichiCount = new KyoutakuRiichiCount(1) };
        var winner = new PlayerIndex(1);
        var loser = new PlayerIndex(3);
        var scoreResults = ImmutableArray.Create(Result((1, 8000), (3, -8000)));

        // Act
        var (settled, _) = round.SettleWin([winner], loser, WinType.Ron, DummyWinTile, scoreResults);

        // Assert
        Assert.Equal(25000 + 8000 + 1000, settled.PointArray[winner].Value);
        Assert.Equal(0, settled.KyoutakuRiichiCount.Value);
    }

    [Fact]
    public void ダブロン_供託は上家取り_両者に計算結果が適用される()
    {
        // Arrange
        var round = CreateBaseRound() with { KyoutakuRiichiCount = new KyoutakuRiichiCount(2) };
        var winners = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(2));
        var loser = new PlayerIndex(3);
        var scoreResults = ImmutableArray.Create(
            Result((1, 8000), (3, -8000)),
            Result((2, 8000), (3, -8000))
        );

        // Act
        var (settled, _) = round.SettleWin(winners, loser, WinType.Ron, DummyWinTile, scoreResults);

        // Assert: winners[0] = index 1 が上家取り (供託 2000)
        Assert.Equal(25000 + 8000 + 2000, settled.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 8000, settled.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 16000, settled.PointArray[loser].Value);
        Assert.Equal(0, settled.KyoutakuRiichiCount.Value);
    }

    [Fact]
    public void ダブロン本場あり_各和了者がそれぞれ本場ボーナスを受け取る()
    {
        // Arrange
        var round = CreateBaseRound() with { Honba = new Honba(1) };
        var winners = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(2));
        var loser = new PlayerIndex(3);
        var scoreResults = ImmutableArray.Create(
            Result((1, 8000), (3, -8000)),
            Result((2, 8000), (3, -8000))
        );

        // Act
        var (settled, _) = round.SettleWin(winners, loser, WinType.Ron, DummyWinTile, scoreResults);

        // Assert: 各和了者が 300×1=300 を放銃者から受け取る (計 600 を放銃者が支払い)
        Assert.Equal(25000 + 8000 + 300, settled.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 8000 + 300, settled.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 16000 - 600, settled.PointArray[loser].Value);
    }

    [Fact]
    public void 和了者空_例外()
    {
        // Arrange
        var round = CreateBaseRound();
        var scoreResults = ImmutableArray.Create(Result());

        // Act
        var exception = Record.Exception(() =>
            round.SettleWin([], new PlayerIndex(0), WinType.Tsumo, DummyWinTile, scoreResults));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 和了者重複_例外()
    {
        // Arrange
        var round = CreateBaseRound();
        var duplicate = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(1));
        var scoreResults = ImmutableArray.Create(Result(), Result());

        // Act
        var exception = Record.Exception(() =>
            round.SettleWin(duplicate, new PlayerIndex(3), WinType.Ron, DummyWinTile, scoreResults));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void WinTileがWinSettlementDetailsに反映される()
    {
        // Arrange: 呼び出し側で明示的に決定した winTile が details.Winners の WinTile に入ることを検証
        var winner = new PlayerIndex(0);
        var loser = new PlayerIndex(3);
        var round = CreateBaseRound();
        var chosenWinTile = new Tile(19);
        var scoreResults = ImmutableArray.Create(Result((0, 8000), (3, -8000)));

        // Act
        var (_, details) = round.SettleWin([winner], loser, WinType.Chankan, chosenWinTile, scoreResults);

        // Assert
        Assert.Equal(chosenWinTile.Id, details.Winners[0].WinTile.Id);
    }
}
