using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_TemporaryFuritenTests
{
    [Fact]
    public void SetTemporaryFuriten_trueを指定_対象プレイヤーのIsTemporaryFuritenがtrueになる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound();
        var target = new PlayerIndex(2);

        // Act
        var result = round.SetTemporaryFuriten(target, true);

        // Assert
        Assert.True(result.PlayerRoundStatusArray[target].IsTemporaryFuriten);
        Assert.False(result.PlayerRoundStatusArray[new PlayerIndex(0)].IsTemporaryFuriten);
        Assert.False(result.PlayerRoundStatusArray[new PlayerIndex(1)].IsTemporaryFuriten);
        Assert.False(result.PlayerRoundStatusArray[new PlayerIndex(3)].IsTemporaryFuriten);
    }

    [Fact]
    public void SetTemporaryFuriten_現状と同値_同一インスタンスを返す()
    {
        // Arrange: 初期状態はすべて false
        var round = RoundTestHelper.CreateRound();

        // Act
        var result = round.SetTemporaryFuriten(new PlayerIndex(0), false);

        // Assert: 副作用なし
        Assert.Same(round, result);
    }

    [Fact]
    public void Tsumo_現手番プレイヤーのIsTemporaryFuritenが解除される()
    {
        // Arrange: Turn=0 プレイヤーに同巡フリテン適用
        var round = RoundTestHelper.CreateRound().Haipai();
        var target = round.Turn;
        round = round.SetTemporaryFuriten(target, true);
        Assert.True(round.PlayerRoundStatusArray[target].IsTemporaryFuriten);

        // Act: 同 Turn でツモ
        var result = round.Tsumo();

        // Assert: 自分のツモで解除
        Assert.False(result.PlayerRoundStatusArray[target].IsTemporaryFuriten);
    }

    [Fact]
    public void Tsumo_他プレイヤーのIsTemporaryFuritenは維持される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var other = round.Turn.Next();
        round = round.SetTemporaryFuriten(other, true);

        // Act: 自分がツモ (他プレイヤー状態は不変)
        var result = round.Tsumo();

        // Assert
        Assert.True(result.PlayerRoundStatusArray[other].IsTemporaryFuriten);
    }
}
