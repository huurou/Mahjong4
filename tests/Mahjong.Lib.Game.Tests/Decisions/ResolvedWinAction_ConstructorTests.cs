using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Decisions;

public class ResolvedWinAction_ConstructorTests
{
    [Fact]
    public void ロン和了_全フィールドが保持される()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(3, 40, new PointArray(new Point(0)), [])
        );
        var award = new KyoutakuRiichiAward(new PlayerIndex(1), 2);

        // Act
        var action = new ResolvedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Ron,
            award,
            new Honba(1),
            false
        );

        // Assert
        Assert.Single(action.WinnerIndices);
        Assert.Equal(new PlayerIndex(1), action.WinnerIndices[0].PlayerIndex);
        Assert.Equal(new PlayerIndex(0), action.LoserIndex);
        Assert.Equal(WinType.Ron, action.WinType);
        Assert.NotNull(action.KyoutakuRiichiAward);
        Assert.Equal(2, action.KyoutakuRiichiAward.Count);
        Assert.Equal(1, action.Honba.Value);
        Assert.False(action.DealerContinues);
    }

    [Fact]
    public void ツモ和了_LoserIndexがnull()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var action = new ResolvedWinAction(
            [winner],
            null,
            WinType.Tsumo,
            null,
            new Honba(0),
            true
        );

        // Assert
        Assert.Null(action.LoserIndex);
        Assert.Null(action.KyoutakuRiichiAward);
        Assert.Equal(WinType.Tsumo, action.WinType);
        Assert.True(action.DealerContinues);
    }

    [Fact]
    public void ダブロン_Winners複数()
    {
        // Arrange
        var scoreResult = new ScoreResult(2, 30, new PointArray(new Point(0)), []);
        var winners = ImmutableList.Create(
            new ResolvedWinner(new PlayerIndex(1), new Tile(0), scoreResult),
            new ResolvedWinner(new PlayerIndex(2), new Tile(0), scoreResult)
        );

        // Act
        var action = new ResolvedWinAction(
            winners,
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 1),
            new Honba(0),
            false
        );

        // Assert
        Assert.Equal(2, action.WinnerIndices.Count);
    }

    [Fact]
    public void ResolvedRoundActionを継承している()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        ResolvedRoundAction action = new ResolvedWinAction(
            [winner],
            null,
            WinType.Tsumo,
            null,
            new Honba(0),
            true
        );

        // Assert
        Assert.IsType<ResolvedRoundAction>(action, exactMatch: false);
    }

    [Fact]
    public void 和了者が空_例外が発生する()
    {
        // Act
        var ex = Record.Exception(() => new ResolvedWinAction(
            [],
            new PlayerIndex(0),
            WinType.Ron,
            null,
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ロン和了でLoserIndexがnull_例外が発生する()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var ex = Record.Exception(() => new ResolvedWinAction(
            [winner],
            null,
            WinType.Ron,
            null,
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 槍槓和了でLoserIndexがnull_例外が発生する()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var ex = Record.Exception(() => new ResolvedWinAction(
            [winner],
            null,
            WinType.Chankan,
            null,
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 槍槓和了_LoserIndexが保持される()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var action = new ResolvedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Chankan,
            null,
            new Honba(0),
            false
        );

        // Assert
        Assert.Equal(new PlayerIndex(0), action.LoserIndex);
        Assert.Equal(WinType.Chankan, action.WinType);
    }

    [Fact]
    public void ツモ和了でLoserIndexが指定されている_例外が発生する()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var ex = Record.Exception(() => new ResolvedWinAction(
            [winner],
            new PlayerIndex(1),
            WinType.Tsumo,
            null,
            new Honba(0),
            true
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 嶺上和了でLoserIndexが指定されている_例外が発生する()
    {
        // Arrange
        var winner = new ResolvedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [])
        );

        // Act
        var ex = Record.Exception(() => new ResolvedWinAction(
            [winner],
            new PlayerIndex(1),
            WinType.Rinshan,
            null,
            new Honba(0),
            true
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
