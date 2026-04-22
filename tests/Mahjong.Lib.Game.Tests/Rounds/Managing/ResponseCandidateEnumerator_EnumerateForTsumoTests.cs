using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseCandidateEnumerator_EnumerateForTsumoTests
{
    [Fact]
    public void 通常の手牌14枚_DahaiCandidateで14通り提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForTsumo(round, round.Turn);

        // Assert
        var dahai = candidates.GetCandidates<DahaiCandidate>().Single();
        Assert.Equal(14, dahai.DahaiOptionList.Count);
    }

    [Fact]
    public void 立直中_DahaiCandidateはツモ切り1通りのみ提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var status = round.PlayerRoundStatusArray[round.Turn] with { IsRiichi = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(round.Turn, status) };
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForTsumo(round, round.Turn);

        // Assert
        var dahai = candidates.GetCandidates<DahaiCandidate>().Single();
        Assert.Single(dahai.DahaiOptionList);
    }

    [Fact]
    public void テンパイ判定false_TsumoAgariCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForTsumo(round, round.Turn);

        // Assert
        Assert.False(candidates.HasCandidate<TsumoAgariCandidate>());
    }

    [Fact]
    public void 手牌に同種4枚_AnkanCandidateが提示される()
    {
        // Arrange: CreateRound の連番ウォールで親は yama[135,134,133,132] (全て kind 33) を取得
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForTsumo(round, round.Turn);

        // Assert
        var ankans = candidates.GetCandidates<AnkanCandidate>().ToImmutableArray();
        Assert.NotEmpty(ankans);
        Assert.All(ankans, x => Assert.Equal(4, x.Tiles.Length));
    }

    [Fact]
    public void 第一打前かつ幺九牌9種以上_KyuushuKyuuhaiCandidateが提示される()
    {
        // Arrange: 9 種の幺九牌 (1m,9m,1p,9p,1s,9s,東,南,西) + 4 枚の非幺九牌で手牌を組む
        var yaochuuTiles = new[]
        {
            new Tile(0), new Tile(32), new Tile(36), new Tile(68), new Tile(72), new Tile(104),
            new Tile(108), new Tile(112), new Tile(116),
        };
        var nonYaochuu = new[] { new Tile(4), new Tile(8), new Tile(12), new Tile(40), new Tile(44) };
        var hand = yaochuuTiles.Concat(nonYaochuu);
        var round = RoundTestHelper.CreateRound().Haipai();
        round = RoundTestHelper.InjectHand(round, round.RoundNumber.ToDealer(), hand);
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForTsumo(round, round.Turn);

        // Assert
        Assert.True(candidates.HasCandidate<KyuushuKyuuhaiCandidate>());
    }
}
