using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class TenhouResponsePriorityPolicy_ResolveTests
{
    private static readonly TenhouResponsePriorityPolicy policy_ = new();

    [Fact]
    public void 放銃者2_ロン0と3_先頭は3()
    {
        // Arrange
        var spec = CreateDahaiSpec(loserIndex: 2);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(0), new RonResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(3), new RonResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(new PlayerIndex(3), result[0].PlayerIndex);
        Assert.Equal(new PlayerIndex(0), result[1].PlayerIndex);
    }

    [Fact]
    public void 放銃者3_ロン0と1_先頭は0()
    {
        // Arrange
        var spec = CreateDahaiSpec(loserIndex: 3);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(1), new RonResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(0), new RonResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(new PlayerIndex(0), result[0].PlayerIndex);
        Assert.Equal(new PlayerIndex(1), result[1].PlayerIndex);
    }

    [Fact]
    public void 単一ロン_そのまま採用される()
    {
        // Arrange
        var spec = CreateDahaiSpec(loserIndex: 2);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(0), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(1), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(3), new RonResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Single(result);
        Assert.Equal(new PlayerIndex(3), result[0].PlayerIndex);
    }

    [Fact]
    public void ポンと大明槓衝突_放銃者基準で上家優先()
    {
        // Arrange: 放銃者 0 に対し 2 がポン、3 が大明槓 → 下家の 1 優先だが 1 は未応答
        // 2 (対面=距離2) と 3 (上家=距離3) なので 2 が優先される
        var spec = CreateDahaiSpec(loserIndex: 0);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(2), new PonResponse([new Tile(0), new Tile(1)])),
            new ResolvedPlayerResponse(new PlayerIndex(3), new DaiminkanResponse([new Tile(0), new Tile(1), new Tile(2)]))
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Single(result);
        Assert.Equal(new PlayerIndex(2), result[0].PlayerIndex);
    }

    [Fact]
    public void チーのみ_採用される()
    {
        // Arrange: 放銃者 2 / 下家 3 がチー
        var spec = CreateDahaiSpec(loserIndex: 2);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(3), new ChiResponse([new Tile(0), new Tile(4)]))
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Single(result);
        Assert.Equal(new PlayerIndex(3), result[0].PlayerIndex);
    }

    [Fact]
    public void 全員スルー_responsesがそのまま返る()
    {
        // Arrange
        var spec = CreateDahaiSpec(loserIndex: 0);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(1), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(2), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(3), new OkResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void 槍槓ロン_Kanフェーズ_放銃者基準順で全員採用()
    {
        // Arrange: 加槓者 2 に対し 3 と 0 が槍槓ロン → 下家 3 先頭
        var spec = CreateKanSpec(loserIndex: 2);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(0), new ChankanRonResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(3), new ChankanRonResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(new PlayerIndex(3), result[0].PlayerIndex);
        Assert.Equal(new PlayerIndex(0), result[1].PlayerIndex);
    }

    [Fact]
    public void Kanフェーズ_全員スルー_そのまま返る()
    {
        // Arrange
        var spec = CreateKanSpec(loserIndex: 0);
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(1), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(2), new OkResponse()),
            new ResolvedPlayerResponse(new PlayerIndex(3), new OkResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void Haipai_単一応答はそのまま返る()
    {
        // Arrange
        var spec = new RoundDecisionSpec(
            RoundDecisionPhase.Haipai,
            [new PlayerDecisionSpec(new PlayerIndex(0), [new OkCandidate()])],
            null
        );
        var responses = ImmutableArray.Create(
            new ResolvedPlayerResponse(new PlayerIndex(0), new OkResponse())
        );

        // Act
        var result = policy_.Resolve(spec, responses);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Dahai_LoserIndexがnull_例外()
    {
        // Arrange
        var specWithoutLoser = new RoundDecisionSpec(
            RoundDecisionPhase.Dahai,
            [new PlayerDecisionSpec(new PlayerIndex(0), [new OkCandidate()])],
            null
        );
        var responses = ImmutableArray<ResolvedPlayerResponse>.Empty;

        // Act
        var ex = Record.Exception(() => policy_.Resolve(specWithoutLoser, responses));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    private static RoundDecisionSpec CreateDahaiSpec(int loserIndex)
    {
        return new RoundDecisionSpec(
            RoundDecisionPhase.Dahai,
            [new PlayerDecisionSpec(new PlayerIndex(loserIndex), [new OkCandidate()])],
            new PlayerIndex(loserIndex)
        );
    }

    private static RoundDecisionSpec CreateKanSpec(int loserIndex)
    {
        return new RoundDecisionSpec(
            RoundDecisionPhase.Kan,
            [new PlayerDecisionSpec(new PlayerIndex(loserIndex), [new OkCandidate()])],
            new PlayerIndex(loserIndex)
        );
    }
}
