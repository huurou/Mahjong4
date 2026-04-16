using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Moq;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_SettleWinTests
{
    private static ScoreResult Result(params (int index, int delta)[] deltas)
    {
        var array = new PointArray(new Point(0));
        foreach (var (index, delta) in deltas)
        {
            array = delta >= 0
                ? array.AddPoint(new PlayerIndex(index), delta)
                : array.SubtractPoint(new PlayerIndex(index), -delta);
        }
        return new ScoreResult(0, 0, array);
    }

    private static Round CreateBaseRound()
    {
        return RoundTestHelper.CreateRound().Haipai()
             with { PointArray = new PointArray(new Point(25000)) };
    }

    [Fact]
    public void ScoreCalculatorが返したPointDeltasがPointArrayに適用される()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(Result((0, 12000), (1, -4000), (2, -4000), (3, -4000)));
        var round = CreateBaseRound();
        var dealer = new PlayerIndex(0);

        // Act
        var result = round.SettleWin([dealer], dealer, WinType.Tsumo, mock.Object);

        // Assert
        Assert.Equal(25000 + 12000, result.PointArray[dealer].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void ScoreCalculatorに渡されるScoreRequestはWinnerとLoserとWinTypeを反映()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        ScoreRequest? captured = null;
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Callback<ScoreRequest>(x => captured = x)
            .Returns(Result());
        var round = CreateBaseRound();
        var winner = new PlayerIndex(1);
        var loser = new PlayerIndex(3);

        // Act
        round.SettleWin([winner], loser, WinType.Ron, mock.Object);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(winner, captured.WinnerIndex);
        Assert.Equal(loser, captured.LoserIndex);
        Assert.Equal(WinType.Ron, captured.WinType);
    }

    [Fact]
    public void 本場1本_親ロンで放銃者から和了者へ300点追加移動()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(Result((0, 12000), (2, -12000)));
        var round = CreateBaseRound() with { Honba = new Honba(1) };
        var dealer = new PlayerIndex(0);
        var loser = new PlayerIndex(2);

        // Act
        var result = round.SettleWin([dealer], loser, WinType.Ron, mock.Object);

        // Assert
        Assert.Equal(25000 + 12000 + 300, result.PointArray[dealer].Value);
        Assert.Equal(25000 - 12000 - 300, result.PointArray[loser].Value);
    }

    [Fact]
    public void 本場2本_親ツモで各他家から100点追加移動()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(Result((0, 12000), (1, -4000), (2, -4000), (3, -4000)));
        var round = CreateBaseRound() with { Honba = new Honba(2) };
        var dealer = new PlayerIndex(0);

        // Act
        var result = round.SettleWin([dealer], dealer, WinType.Tsumo, mock.Object);

        // Assert
        Assert.Equal(25000 + 12000 + 600, result.PointArray[dealer].Value);
        Assert.Equal(25000 - 4000 - 200, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000 - 200, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000 - 200, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 供託1本_和了者に1000点加算され供託は0になる()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(Result((1, 8000), (3, -8000)));
        var round = CreateBaseRound() with { KyoutakuRiichiCount = new KyoutakuRiichiCount(1) };
        var winner = new PlayerIndex(1);
        var loser = new PlayerIndex(3);

        // Act
        var result = round.SettleWin([winner], loser, WinType.Ron, mock.Object);

        // Assert
        Assert.Equal(25000 + 8000 + 1000, result.PointArray[winner].Value);
        Assert.Equal(0, result.KyoutakuRiichiCount.Value);
    }

    [Fact]
    public void ダブロン_供託は上家取り_両者に計算結果が適用される()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.Is<ScoreRequest>(r => r.WinnerIndex == new PlayerIndex(1))))
            .Returns(Result((1, 8000), (3, -8000)));
        mock.Setup(x => x.Calculate(It.Is<ScoreRequest>(r => r.WinnerIndex == new PlayerIndex(2))))
            .Returns(Result((2, 8000), (3, -8000)));
        var round = CreateBaseRound() with { KyoutakuRiichiCount = new KyoutakuRiichiCount(2) };
        var winners = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(2));
        var loser = new PlayerIndex(3);

        // Act
        var result = round.SettleWin(winners, loser, WinType.Ron, mock.Object);

        // Assert: winners[0] = index 1 が上家取り (供託 2000)
        Assert.Equal(25000 + 8000 + 2000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 8000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 16000, result.PointArray[loser].Value);
        Assert.Equal(0, result.KyoutakuRiichiCount.Value);
    }

    [Fact]
    public void ダブロン本場あり_各和了者がそれぞれ本場ボーナスを受け取る()
    {
        // Arrange
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.Is<ScoreRequest>(r => r.WinnerIndex == new PlayerIndex(1))))
            .Returns(Result((1, 8000), (3, -8000)));
        mock.Setup(x => x.Calculate(It.Is<ScoreRequest>(r => r.WinnerIndex == new PlayerIndex(2))))
            .Returns(Result((2, 8000), (3, -8000)));
        var round = CreateBaseRound() with { Honba = new Honba(1) };
        var winners = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(2));
        var loser = new PlayerIndex(3);

        // Act
        var result = round.SettleWin(winners, loser, WinType.Ron, mock.Object);

        // Assert: 各和了者が 300×1=300 を放銃者から受け取る (計 600 を放銃者が支払い)
        Assert.Equal(25000 + 8000 + 300, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 8000 + 300, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 16000 - 600, result.PointArray[loser].Value);
    }

    [Fact]
    public void 和了者空_例外()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var exception = Record.Exception(() =>
            round.SettleWin([], new PlayerIndex(0), WinType.Tsumo, RoundTestHelper.NoOpScoreCalculator));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 和了者重複_例外()
    {
        // Arrange
        var round = CreateBaseRound();
        var duplicate = ImmutableArray.Create(new PlayerIndex(1), new PlayerIndex(1));

        // Act
        var exception = Record.Exception(() =>
            round.SettleWin(duplicate, new PlayerIndex(3), WinType.Ron, RoundTestHelper.NoOpScoreCalculator));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}
