using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_SettleWinUradoraTests
{
    private static Tile DummyWinTile { get; } = new(0);

    private static ScoreResult EmptyResult() => new(0, 0, new PointArray(new Point(0)), [], IsMenzen: true);

    private static Round CreateBaseRound()
    {
        return RoundTestHelper.CreateRound().Haipai() with
        {
            PointArray = new PointArray(new Point(25000)),
        };
    }

    [Fact]
    public void 立直者が和了に含まれる_表示中のドラ枚数分の裏ドラが返る()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var round = CreateBaseRound();
        var riichiStatus = round.PlayerRoundStatusArray[winner] with { IsRiichi = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(winner, riichiStatus) };
        var scoreResults = ImmutableArray.Create(EmptyResult());

        // Act
        var (_, details) = round.SettleWin([winner], winner, WinType.Tsumo, DummyWinTile, scoreResults);

        // Assert
        Assert.Equal(round.Wall.DoraRevealedCount, details.UraDoraIndicators.Length);
        Assert.Equal(round.Wall.GetUradoraIndicator(0), details.UraDoraIndicators[0]);
    }

    [Fact]
    public void ダブルリーチ者が和了に含まれる_裏ドラが返る()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var round = CreateBaseRound();
        var doubleRiichiStatus = round.PlayerRoundStatusArray[winner] with { IsDoubleRiichi = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(winner, doubleRiichiStatus) };
        var scoreResults = ImmutableArray.Create(EmptyResult());

        // Act
        var (_, details) = round.SettleWin([winner], winner, WinType.Tsumo, DummyWinTile, scoreResults);

        // Assert
        Assert.NotEmpty(details.UraDoraIndicators);
    }

    [Fact]
    public void 立直者なしの和了_UraDoraIndicatorsは空配列()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var round = CreateBaseRound();
        var scoreResults = ImmutableArray.Create(EmptyResult());

        // Act
        var (_, details) = round.SettleWin([winner], winner, WinType.Tsumo, DummyWinTile, scoreResults);

        // Assert
        Assert.Empty(details.UraDoraIndicators);
    }

    [Fact]
    public void ダブロンで和了者のうち立直者のみ_UraDoraIndicatorsが取得される()
    {
        // Arrange: 和了者 index=0 (立直なし) と index=1 (立直あり) が同時ロン
        var nonRiichi = new PlayerIndex(0);
        var riichi = new PlayerIndex(1);
        var round = CreateBaseRound();
        var riichiStatus = round.PlayerRoundStatusArray[riichi] with { IsRiichi = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(riichi, riichiStatus) };
        var scoreResults = ImmutableArray.Create(EmptyResult(), EmptyResult());

        // Act
        var (_, details) = round.SettleWin([nonRiichi, riichi], new PlayerIndex(3), WinType.Ron, DummyWinTile, scoreResults);

        // Assert: 1 人でも立直者が居れば裏ドラを返す
        Assert.NotEmpty(details.UraDoraIndicators);
    }
}
