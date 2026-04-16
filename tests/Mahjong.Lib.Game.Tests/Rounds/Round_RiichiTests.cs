using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_PendRiichiTests
{
    [Fact]
    public void 立直保留_持ち点と供託は変わらず_PendingRiichiPlayerに記録される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        var initialPoint = round.PointArray[playerIndex].Value;
        var initialKyoutaku = round.KyoutakuRiichiCount.Value;

        // Act
        var result = round.PendRiichi(playerIndex);

        // Assert
        Assert.Equal(initialPoint, result.PointArray[playerIndex].Value);
        Assert.Equal(initialKyoutaku, result.KyoutakuRiichiCount.Value);
        Assert.Equal(playerIndex, result.PendingRiichiPlayerIndex);
        // フラグもまだ未確定
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsRiichi);
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsIppatsu);
    }

    [Fact]
    public void 既に立直保留中_例外()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        round = round.PendRiichi(round.Turn);

        // Act
        var exception = Record.Exception(() => round.PendRiichi(new PlayerIndex(1)));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 持ち点1000未満_例外()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        round = round with { PointArray = round.PointArray.SubtractPoint(playerIndex, 24500) };   // 残 500

        // Act
        var exception = Record.Exception(() => round.PendRiichi(playerIndex));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 既に立直済み_例外()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        round = round.PendRiichi(playerIndex).ConfirmRiichi();

        // Act
        var exception = Record.Exception(() => round.PendRiichi(playerIndex));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
}

public class Round_ConfirmRiichiTests
{
    [Fact]
    public void 第一打前で確定_IsRiichiとIsDoubleRiichiとIsIppatsuがtrue_持ち点1000減_供託1増()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        var initialPoint = round.PointArray[playerIndex].Value;
        round = round.PendRiichi(playerIndex);

        // Act
        var result = round.ConfirmRiichi();

        // Assert
        var status = result.PlayerRoundStatusArray[playerIndex];
        Assert.True(status.IsRiichi);
        Assert.True(status.IsDoubleRiichi);
        Assert.True(status.IsIppatsu);
        Assert.Equal(initialPoint - 1000, result.PointArray[playerIndex].Value);
        Assert.Equal(1, result.KyoutakuRiichiCount.Value);
        Assert.Null(result.PendingRiichiPlayerIndex);
    }

    [Fact]
    public void 鳴き発生後の確定_IsDoubleRiichiはfalse()
    {
        // Arrange: 鳴きで全員の IsFirstTurnBeforeDiscard が落ちる
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        round = round.Chi(caller, System.Collections.Immutable.ImmutableList.Create(new Tile(84), new Tile(88)));
        var laterPlayer = new PlayerIndex(3);
        round = round.PendRiichi(laterPlayer);

        // Act
        var result = round.ConfirmRiichi();

        // Assert
        var status = result.PlayerRoundStatusArray[laterPlayer];
        Assert.True(status.IsRiichi);
        Assert.False(status.IsDoubleRiichi);
        Assert.True(status.IsIppatsu);
    }

    [Fact]
    public void 保留なし_状態変わらず()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var initialPoint = round.PointArray[new PlayerIndex(0)].Value;

        // Act
        var result = round.ConfirmRiichi();

        // Assert: 何も変わらない
        Assert.Equal(initialPoint, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(0, result.KyoutakuRiichiCount.Value);
    }
}

public class Round_CancelRiichiTests
{
    [Fact]
    public void 保留中の立直を破棄_持ち点と供託は変わらない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        var initialPoint = round.PointArray[playerIndex].Value;
        var initialKyoutaku = round.KyoutakuRiichiCount.Value;
        round = round.PendRiichi(playerIndex);

        // Act
        var result = round.CancelRiichi();

        // Assert
        Assert.Null(result.PendingRiichiPlayerIndex);
        Assert.Equal(initialPoint, result.PointArray[playerIndex].Value);
        Assert.Equal(initialKyoutaku, result.KyoutakuRiichiCount.Value);
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsRiichi);
    }

    [Fact]
    public void 保留なしでCancel_状態変わらず()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();

        // Act
        var result = round.CancelRiichi();

        // Assert
        Assert.Null(result.PendingRiichiPlayerIndex);
    }
}
