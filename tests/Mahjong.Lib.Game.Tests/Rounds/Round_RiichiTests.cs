using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

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
        round = round.Dahai(new Tile(83));
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        round = round.Chi(caller, ImmutableList.Create(new Tile(84), new Tile(88)));
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

    [Fact]
    public void 第一打前で確定_SafeKindsAgainstRiichiは空集合()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = round.Turn;
        round = round.PendRiichi(playerIndex);

        // Act
        var result = round.ConfirmRiichi();

        // Assert: まだ誰も捨てていないので空集合
        var status = result.PlayerRoundStatusArray[playerIndex];
        Assert.NotNull(status.SafeKindsAgainstRiichi);
        Assert.Empty(status.SafeKindsAgainstRiichi);
    }

    [Fact]
    public void 数巡経過後に確定_SafeKindsAgainstRiichiに自分の河の牌種が記録される()
    {
        // Arrange: 各プレイヤーが 1 手ずつ打牌したあと、親がもう 1 手進めて立直
        var round = RoundTestHelper.CreateRound().Haipai();
        // 親 (P0)
        var t0 = round.HandArray[round.Turn].First();
        round = round.Dahai(t0).NextTurn();
        // P1
        round = round.Tsumo();
        var t1 = round.HandArray[round.Turn].Last();
        round = round.Dahai(t1).NextTurn();
        // P2
        round = round.Tsumo();
        var t2 = round.HandArray[round.Turn].Last();
        round = round.Dahai(t2).NextTurn();
        // P3
        round = round.Tsumo();
        var t3 = round.HandArray[round.Turn].Last();
        round = round.Dahai(t3).NextTurn();
        // 親が 2 巡目ツモ → 立直
        round = round.Tsumo();
        var riichiPlayer = round.Turn;
        var tr = round.HandArray[riichiPlayer].Last();
        round = round.Dahai(tr);
        round = round.PendRiichi(riichiPlayer);

        // Act
        var result = round.ConfirmRiichi();

        // Assert: 親の河 (t0, tr) の牌種が自分の SafeKindsAgainstRiichi に記録される (振り聴)
        var status = result.PlayerRoundStatusArray[riichiPlayer];
        Assert.NotNull(status.SafeKindsAgainstRiichi);
        Assert.Contains(t0.Kind, status.SafeKindsAgainstRiichi);
        Assert.Contains(tr.Kind, status.SafeKindsAgainstRiichi);
    }

    [Fact]
    public void 立直確定後の他家打牌_SafeKindsAgainstRiichiに追加される()
    {
        // Arrange: 親が立直確定 → P1 が何か捨てる
        var round = RoundTestHelper.CreateRound().Haipai();
        var riichiPlayer = round.Turn;
        round = round.PendRiichi(riichiPlayer).ConfirmRiichi();
        // リーチ確定直後 (自分の河も空) の時点で、P1 がツモ打牌を行う
        round = round.NextTurn().Tsumo();
        var t1 = round.HandArray[round.Turn].Last();

        // Act
        round = round.Dahai(t1);

        // Assert: リーチ者の SafeKindsAgainstRiichi に P1 の打牌牌種が追加される
        var status = round.PlayerRoundStatusArray[riichiPlayer];
        Assert.NotNull(status.SafeKindsAgainstRiichi);
        Assert.Contains(t1.Kind, status.SafeKindsAgainstRiichi);
    }

    [Fact]
    public void 立直確定後_立直者自身のツモ切り打牌_SafeKindsAgainstRiichiに追加される()
    {
        // Arrange: 親が立直確定直後、他3家を素通りさせて親の次ツモ番まで進める
        var round = RoundTestHelper.CreateRound().Haipai();
        var riichiPlayer = round.Turn;
        round = round.PendRiichi(riichiPlayer).ConfirmRiichi();
        round = round.NextTurn();
        for (var i = 0; i < 3; i++)
        {
            round = round.Tsumo();
            round = round.Dahai(round.HandArray[round.Turn].Last()).NextTurn();
        }
        // 親 (リーチ者) 自身の次のツモ → ツモ切り
        round = round.Tsumo();
        var selfTile = round.HandArray[round.Turn].Last();

        // Act
        round = round.Dahai(selfTile);

        // Assert: 振り聴ルールにより立直者自身の打牌種も自身の SafeKindsAgainstRiichi に追加される
        var status = round.PlayerRoundStatusArray[riichiPlayer];
        Assert.NotNull(status.SafeKindsAgainstRiichi);
        Assert.Contains(selfTile.Kind, status.SafeKindsAgainstRiichi);
    }

    [Fact]
    public void 立直前に鳴かれた自分の打牌_ConfirmRiichiの初期SafeKindsAgainstRiichiに含まれる()
    {
        // Arrange: 親が s3 (Tile83) を打牌 → 子がチーで鳴く。
        // チー後 Tile(83) は親の河から消え、TilesCalledFromRiver に移動する
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var riichiPlayer = round.Turn;
        round = RoundTestHelper.InjectHand(round, riichiPlayer,
        [
            new Tile(4), new Tile(8), new Tile(12),     // m234
            new Tile(16), new Tile(20), new Tile(24),   // m567
            new Tile(40), new Tile(44), new Tile(48),   // p234
            new Tile(52), new Tile(53),                  // p5 p5
            new Tile(76), new Tile(84),                  // s2 s4
            new Tile(83),                                 // s3 (打牌予定、14枚目)
        ]);
        round = round.Dahai(new Tile(83));
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(85), new Tile(88),                  // s4 (caller用), s5
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(5), new Tile(6), new Tile(7),
            new Tile(11), new Tile(14), new Tile(15), new Tile(17),
        ]);
        round = round.Chi(caller, ImmutableList.Create(new Tile(85), new Tile(88)));
        // 親が立直保留 (PendRiichi は持ち点検査のみ、テンパイ形は不要)
        round = round.PendRiichi(riichiPlayer);

        // Act
        var result = round.ConfirmRiichi();

        // Assert: 鳴かれた Tile(83) の牌種 (s3 = kind 20) が初期 SafeKindsAgainstRiichi に含まれる
        var status = result.PlayerRoundStatusArray[riichiPlayer];
        Assert.NotNull(status.SafeKindsAgainstRiichi);
        Assert.Contains(new Tile(83).Kind, status.SafeKindsAgainstRiichi);
    }

    [Fact]
    public void 保留なし_状態変わらず_SafeKindsAgainstRiichiはnullのまま()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = new PlayerIndex(0);

        // Act
        var result = round.ConfirmRiichi();

        // Assert: 立直保留なしなら SafeKindsAgainstRiichi は未設定のまま
        Assert.Null(result.PlayerRoundStatusArray[playerIndex].SafeKindsAgainstRiichi);
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
