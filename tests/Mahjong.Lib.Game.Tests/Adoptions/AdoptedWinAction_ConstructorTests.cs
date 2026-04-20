using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Adoptions;

public class AdoptedWinAction_ConstructorTests
{
    [Fact]
    public void ロン和了_全フィールドが保持される()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(3, 40, new PointArray(new Point(0)), [], IsMenzen: false)
        );
        var award = new KyoutakuRiichiAward(new PlayerIndex(1), 2);

        // Act
        var action = new AdoptedWinAction(
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
    public void ツモ和了_LoserIndexに和了者自身を保持する()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            true
        );

        // Assert
        Assert.Equal(new PlayerIndex(0), action.LoserIndex);
        Assert.Equal(0, action.KyoutakuRiichiAward.Count);
        Assert.Equal(WinType.Tsumo, action.WinType);
        Assert.True(action.DealerContinues);
    }

    [Fact]
    public void ダブロン_Winners複数()
    {
        // Arrange
        var scoreResult = new ScoreResult(2, 30, new PointArray(new Point(0)), [], IsMenzen: false);
        var winners = ImmutableList.Create(
            new AdoptedWinner(new PlayerIndex(1), new Tile(0), scoreResult),
            new AdoptedWinner(new PlayerIndex(2), new Tile(0), scoreResult)
        );

        // Act
        var action = new AdoptedWinAction(
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
    public void AdoptedRoundActionを継承している()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        AdoptedRoundAction action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            true
        );

        // Assert
        Assert.IsType<AdoptedRoundAction>(action, exactMatch: false);
    }

    [Fact]
    public void 和了者が空_例外が発生する()
    {
        // Act
        var ex = Record.Exception(() => new AdoptedWinAction(
            [],
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
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
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Chankan,
            new KyoutakuRiichiAward(new PlayerIndex(1), 0),
            new Honba(0),
            false
        );

        // Assert
        Assert.Equal(new PlayerIndex(0), action.LoserIndex);
        Assert.Equal(WinType.Chankan, action.WinType);
    }

    [Fact]
    public void 嶺上和了_LoserIndexに和了者自身を保持する()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Rinshan,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            true
        );

        // Assert
        Assert.Equal(new PlayerIndex(0), action.LoserIndex);
        Assert.Equal(WinType.Rinshan, action.WinType);
    }

    [Fact]
    public void ツモ和了でLoserIndexが和了者以外_例外が発生する()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var ex = Record.Exception(() => new AdoptedWinAction(
            [winner],
            new PlayerIndex(1),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 嶺上和了でLoserIndexが和了者以外_例外が発生する()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(2),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var ex = Record.Exception(() => new AdoptedWinAction(
            [winner],
            new PlayerIndex(3),
            WinType.Rinshan,
            new KyoutakuRiichiAward(new PlayerIndex(2), 0),
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ロン和了でLoserIndexが和了者と同じ_例外が発生する()
    {
        // Arrange
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30, new PointArray(new Point(0)), [], IsMenzen: false)
        );

        // Act
        var ex = Record.Exception(() => new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 槍槓和了でダブロン中に和了者のひとりがLoserIndexと同じ_例外が発生する()
    {
        // Arrange
        var scoreResult = new ScoreResult(2, 30, new PointArray(new Point(0)), [], IsMenzen: false);
        var winners = ImmutableList.Create(
            new AdoptedWinner(new PlayerIndex(1), new Tile(0), scoreResult),
            new AdoptedWinner(new PlayerIndex(2), new Tile(0), scoreResult)
        );

        // Act
        var ex = Record.Exception(() => new AdoptedWinAction(
            winners,
            new PlayerIndex(1),
            WinType.Chankan,
            new KyoutakuRiichiAward(new PlayerIndex(1), 0),
            new Honba(0),
            false
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
