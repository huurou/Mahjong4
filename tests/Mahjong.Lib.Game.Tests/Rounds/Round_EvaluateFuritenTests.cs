using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Moq;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_EvaluateFuritenTests
{
    [Fact]
    public void 待ち牌が河にある_IsFuritenがtrue()
    {
        // Arrange: 親が打牌した牌の Kind を待ち牌とする
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        var dahaiTile = round.HandArray[playerIndex].Last();
        round = round.Dahai(dahaiTile, RoundTestHelper.NoOpTenpaiChecker);

        var checker = new Mock<ITenpaiChecker>();
        checker.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([dahaiTile.Kind]);

        // Act
        var result = round.EvaluateFuriten(playerIndex, checker.Object);

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が河にない_IsFuritenはfalse()
    {
        // Arrange: テンパイ判定機 (待ち kind = 33 = 中)
        var checker = new Mock<ITenpaiChecker>();
        checker.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([33]);

        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        var dahaiTile = round.HandArray[playerIndex].Last();
        round = round.Dahai(dahaiTile, checker.Object);

        // Act
        var result = round.EvaluateFuriten(playerIndex, checker.Object);

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が空_IsFuritenはfalse()
    {
        // Arrange
        var checker = new Mock<ITenpaiChecker>();
        checker.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([]);

        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        round = round.Dahai(round.HandArray[playerIndex].Last(), checker.Object);

        // Act
        var result = round.EvaluateFuriten(playerIndex, checker.Object);

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 打牌時にフリテン評価が自動で行われる()
    {
        // Arrange: 打牌した牌の Kind を待ち牌とする
        var checker = new Mock<ITenpaiChecker>();
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        var dahaiTile = round.HandArray[playerIndex].Last();
        checker.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([dahaiTile.Kind]);

        // Act: Dahai 内で EvaluateFuriten(Turn) が呼ばれる
        var result = round.Dahai(dahaiTile, checker.Object);

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が鳴かれた牌にある_IsFuritenがtrue()
    {
        // Arrange: 親が打牌 → 子がチー。親の河からは消えるが鳴かれた牌として記録される
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var dealerIndex = round.Turn;
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);   // kind 20 = 3索
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        round = round.Chi(caller, ImmutableList.Create(new Tile(84), new Tile(88)));

        // 親 (dealer) の待ちを kind 20 と設定。河は空 (鳴かれた)、TilesCalledFromRiver に Tile(83)
        var dealerHand = round.HandArray[dealerIndex];
        var checker = new Mock<ITenpaiChecker>();
        checker.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([]);
        checker.Setup(x => x.EnumerateWaitTileKinds(dealerHand, It.IsAny<CallList>()))
            .Returns([20]);

        // Act
        var result = round.EvaluateFuriten(dealerIndex, checker.Object);

        // Assert: 親は鳴かれた牌が自分の待ちに含まれるためフリテン
        Assert.True(result.PlayerRoundStatusArray[dealerIndex].IsFuriten);
    }
}
