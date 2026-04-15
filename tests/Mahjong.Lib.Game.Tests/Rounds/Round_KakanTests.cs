using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_KakanTests
{
    [Fact]
    public void Kakan_既存のポンが加槓に差し替わり手牌から加槓牌が除かれる()
    {
        // Arrange
        // 親 P0 がダミー打牌 → P1 がポン → P1 ツモ → P1 加槓のシナリオ
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        // P0は手牌を打牌する。P0の手牌 yama[87] = kind 21 (1索)。
        // P1 は手牌に kind 21 があるか確認: P1の手牌 yama[131..128, 115..112, 99..96, 86] → 86/4=21 のみ1枚しかない
        // そのため P1 はポン不可。代わりにテスト用に手動でポンを仕立てる。
        // P1 がポンを持っている状態を手動で作る: P0 の打牌 87 を P1 が手牌の2枚 (仮に Tile(85), Tile(84) としないと kind 合わないので)
        // 現実的には、既存のPonを手動で CallListArray に追加して、Kakan をテストする。
        var pon = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            new PlayerIndex(0),
            new Tile(87)
        );
        var p1 = new PlayerIndex(1);
        var callListArray = round.CallListArray.AddCall(p1, pon);
        // P1 が手牌に kind 21 の牌 (Tile(86)) を持っており、それを加槓
        round = round with { CallListArray = callListArray, Turn = p1 };

        // Act
        var result = round.Kakan(new Tile(86));

        // Assert
        Assert.DoesNotContain(new Tile(86), result.HandArray[p1]);
        Assert.Single(result.CallListArray[p1]);
        var kakan = result.CallListArray[p1].First();
        Assert.Equal(CallType.Kakan, kakan.Type);
        Assert.Equal(4, kakan.Tiles.Count);
        Assert.Contains(new Tile(86), kakan.Tiles);
    }

    [Fact]
    public void Kakan_対応ポンが無い_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        // P0 の手牌には Tile(87) (kind 21) があるが、対応するポンは無い

        // Act
        var ex = Record.Exception(() => round.Kakan(new Tile(87)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }
}
