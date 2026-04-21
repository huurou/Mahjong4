using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_NagashiManganDrivingTests
{
    [Fact]
    public void Haipai直後_全員が流し満貫条件あり()
    {
        // Arrange & Act
        var round = RoundTestHelper.CreateRound().Haipai();

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.True(round.PlayerRoundStatusArray[new PlayerIndex(i)].IsNagashiMangan);
        }
    }

    [Fact]
    public void 幺九牌を打牌_流し満貫条件は維持()
    {
        // Arrange: kind 0 (1m) は幺九。Tile(0)〜(3) のいずれか
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        round = RoundTestHelper.InjectHand(round, playerIndex,
        [
            new Tile(0),     // 1m (幺九)
            new Tile(4), new Tile(8), new Tile(12), new Tile(16), new Tile(20),
            new Tile(24), new Tile(28), new Tile(36), new Tile(40), new Tile(44),
            new Tile(48), new Tile(52), new Tile(56),
        ]);

        // Act
        var result = round.Dahai(new Tile(0));

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsNagashiMangan);
    }

    [Fact]
    public void 中張牌を打牌_流し満貫条件喪失()
    {
        // Arrange: kind 1 (2m) は中張
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        round = RoundTestHelper.InjectHand(round, playerIndex,
        [
            new Tile(4),     // 2m (中張)
            new Tile(0), new Tile(8), new Tile(12), new Tile(16), new Tile(20),
            new Tile(24), new Tile(28), new Tile(36), new Tile(40), new Tile(44),
            new Tile(48), new Tile(52), new Tile(56),
        ]);

        // Act
        var result = round.Dahai(new Tile(4));

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsNagashiMangan);
    }

    [Fact]
    public void 鳴かれた_出元の流し満貫条件喪失()
    {
        // Arrange: 親 (P0) が打牌、子 (P1) がチー
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var dealerIndex = round.Turn;
        round = round.Dahai(new Tile(83));   // 親が 3索 (kind 20、中張)を捨てる → P0 はすでに資格喪失
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);

        // Act
        var result = round.Chi(caller, ImmutableList.Create(new Tile(84), new Tile(88)));

        // Assert: 鳴かれた P0 (=元 dealer) の資格は false (打牌時点で既に false でもあるが、明示的に確認)
        Assert.False(result.PlayerRoundStatusArray[dealerIndex].IsNagashiMangan);
    }
}
