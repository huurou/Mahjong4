using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_RinshanDrivingTests
{
    [Fact]
    public void RinshanTsumo_当該プレイヤーのIsRinshanがtrueになる()
    {
        // Arrange: 親が暗槓
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        // Player の手牌を kind 0 (1m) 4枚を含む形にする
        round = RoundTestHelper.InjectHand(round, playerIndex,
        [
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(8), new Tile(12), new Tile(16), new Tile(20),
            new Tile(24), new Tile(28), new Tile(36), new Tile(40),
            new Tile(44), new Tile(48),
        ]);
        round = round.Ankan(new Tile(0));

        // Act
        var result = round.RinshanTsumo();

        // Assert
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsRinshan);
    }

    [Fact]
    public void RinshanTsumo後に打牌_IsRinshanがfalseに戻る()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;
        round = RoundTestHelper.InjectHand(round, playerIndex,
        [
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(8), new Tile(12), new Tile(16), new Tile(20),
            new Tile(24), new Tile(28), new Tile(36), new Tile(40),
            new Tile(44), new Tile(48),
        ]);
        round = round.Ankan(new Tile(0)).RinshanTsumo();
        Assert.True(round.PlayerRoundStatusArray[playerIndex].IsRinshan);

        // Act
        var result = round.Dahai(round.HandArray[playerIndex].Last());

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsRinshan);
    }
}
