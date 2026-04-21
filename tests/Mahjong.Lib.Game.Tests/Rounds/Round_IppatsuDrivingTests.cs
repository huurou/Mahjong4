using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_IppatsuDrivingTests
{
    [Fact]
    public void 立直直後_当該プレイヤーのIsIppatsuがtrue()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;

        // Act
        var result = round.PendRiichi(playerIndex).ConfirmRiichi();

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsIppatsu);
    }

    [Fact]
    public void ツモ直後_一発フラグが維持される()
    {
        // Arrange: 立直確定後、まだ打牌していない状態でツモ
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        round = round.PendRiichi(playerIndex).ConfirmRiichi();
        Assert.True(round.PlayerRoundStatusArray[playerIndex].IsIppatsu);

        // Act: ツモ直後は一発フラグ維持 (一発ツモ和了を可能にする)
        var result = round.Tsumo();

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsIppatsu);
    }

    [Fact]
    public void 打牌後_一発フラグが解除される()
    {
        // Arrange: 立直確定後、ツモして打牌
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        round = round.PendRiichi(playerIndex).ConfirmRiichi();
        round = round.Tsumo();
        Assert.True(round.PlayerRoundStatusArray[playerIndex].IsIppatsu);

        // Act: 打牌すると一発は消える (= ツモ和了しなかった)
        var result = round.Dahai(round.HandArray[playerIndex].Last());

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsIppatsu);
    }

    [Fact]
    public void 鳴き発生_全員のIsIppatsuが解除される()
    {
        // Arrange: 立直してから鳴き
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var dealerIndex = round.Turn;
        round = round.PendRiichi(dealerIndex).ConfirmRiichi();
        Assert.True(round.PlayerRoundStatusArray[dealerIndex].IsIppatsu);
        round = round.Dahai(new Tile(83));
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

        // Assert: 全員 false
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.False(result.PlayerRoundStatusArray[new PlayerIndex(i)].IsIppatsu);
        }
    }
}
