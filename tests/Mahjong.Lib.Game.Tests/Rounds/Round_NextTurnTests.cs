namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_NextTurnTests
{
    [Fact]
    public void NextTurn_Turnが次のプレイヤーになる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.NextTurn();

        // Assert
        Assert.Equal(1, result.Turn.Value);
    }

    [Fact]
    public void NextTurn_四回繰り返すと元に戻る()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.NextTurn().NextTurn().NextTurn().NextTurn();

        // Assert
        Assert.Equal(0, result.Turn.Value);
    }
}
