using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Tests.Players;

public class AI_v0_4_0_回し打ち_CalcDangerAgainstTests
{
    [Fact]
    public void SafeKindsに含まれる牌種_危険度0を返す()
    {
        // Arrange
        var tile = new Tile(TileKind.Man1.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man1);
        var view = CreateView();

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, view);

        // Assert
        Assert.Equal(0, danger);
    }

    [Fact]
    public void 字牌_生牌_危険度3を返す()
    {
        // Arrange: 東を自手牌に含めず unseen=4 (ドラ/河/副露にも無し)
        var tonTile = new Tile(TileKind.Ton.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;
        var view = CreateView();

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tonTile, safeKinds, view);

        // Assert
        Assert.Equal(3, danger);
    }

    [Fact]
    public void 字牌_2枚見え_危険度2を返す()
    {
        // Arrange: 東を自手牌に 2 枚入れて unseen=2
        var tonTile = new Tile(TileKind.Ton.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;
        var hand = new Hand([new Tile(TileKind.Ton.Value * 4), new Tile(TileKind.Ton.Value * 4 + 1)]);
        var view = CreateView(ownHand: hand);

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tonTile, safeKinds, view);

        // Assert
        Assert.Equal(2, danger);
    }

    [Fact]
    public void 字牌_3枚見え_危険度1を返す()
    {
        // Arrange: 東を自手牌に 3 枚入れて unseen=1
        var tonTile = new Tile(TileKind.Ton.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;
        var hand = new Hand(
        [
            new Tile(TileKind.Ton.Value * 4),
            new Tile(TileKind.Ton.Value * 4 + 1),
            new Tile(TileKind.Ton.Value * 4 + 2),
        ]);
        var view = CreateView(ownHand: hand);

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tonTile, safeKinds, view);

        // Assert
        Assert.Equal(1, danger);
    }

    [Fact]
    public void 字牌_ラス牌_危険度0を返す()
    {
        // Arrange: 東を自手牌に 4 枚入れて unseen=0
        var tonTile = new Tile(TileKind.Ton.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;
        var hand = new Hand(
        [
            new Tile(TileKind.Ton.Value * 4),
            new Tile(TileKind.Ton.Value * 4 + 1),
            new Tile(TileKind.Ton.Value * 4 + 2),
            new Tile(TileKind.Ton.Value * 4 + 3),
        ]);
        var view = CreateView(ownHand: hand);

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tonTile, safeKinds, view);

        // Assert
        Assert.Equal(0, danger);
    }

    [Fact]
    public void 数牌1_スジあり_危険度3を返す()
    {
        // Arrange: 1m の safeKinds に 4m あり
        var tile = new Tile(TileKind.Man1.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man4);

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        // Assert
        Assert.Equal(3, danger);
    }

    [Fact]
    public void 数牌1_スジなし_危険度6を返す()
    {
        // Arrange
        var tile = new Tile(TileKind.Man1.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        // Assert
        Assert.Equal(6, danger);
    }

    [Fact]
    public void 数牌2_スジあり_危険度4を返す()
    {
        var tile = new Tile(TileKind.Man2.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man5);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(4, danger);
    }

    [Fact]
    public void 数牌2_スジなし_危険度8を返す()
    {
        var tile = new Tile(TileKind.Man2.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(8, danger);
    }

    [Fact]
    public void 数牌3_スジあり_危険度5を返す()
    {
        var tile = new Tile(TileKind.Man3.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man6);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(5, danger);
    }

    [Fact]
    public void 数牌3_スジなし_危険度8を返す()
    {
        var tile = new Tile(TileKind.Man3.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(8, danger);
    }

    [Fact]
    public void 数牌4_両スジ_危険度4を返す()
    {
        var tile = new Tile(TileKind.Man4.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man1, TileKind.Man7);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(4, danger);
    }

    [Fact]
    public void 数牌4_片スジ下のみ_危険度8を返す()
    {
        var tile = new Tile(TileKind.Man4.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man1);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(8, danger);
    }

    [Fact]
    public void 数牌4_片スジ上のみ_危険度8を返す()
    {
        var tile = new Tile(TileKind.Man4.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man7);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(8, danger);
    }

    [Fact]
    public void 数牌4_無スジ_危険度12を返す()
    {
        var tile = new Tile(TileKind.Man4.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(12, danger);
    }

    [Fact]
    public void 数牌5_両スジ_危険度4を返す()
    {
        var tile = new Tile(TileKind.Man5.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man2, TileKind.Man8);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(4, danger);
    }

    [Fact]
    public void 数牌5_無スジ_危険度12を返す()
    {
        var tile = new Tile(TileKind.Man5.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(12, danger);
    }

    [Fact]
    public void 数牌6_両スジ_危険度4を返す()
    {
        var tile = new Tile(TileKind.Man6.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man3, TileKind.Man9);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(4, danger);
    }

    [Fact]
    public void 数牌7_スジあり_危険度5を返す()
    {
        var tile = new Tile(TileKind.Man7.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man4);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(5, danger);
    }

    [Fact]
    public void 数牌7_スジなし_危険度8を返す()
    {
        var tile = new Tile(TileKind.Man7.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(8, danger);
    }

    [Fact]
    public void 数牌8_スジあり_危険度4を返す()
    {
        var tile = new Tile(TileKind.Man8.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man5);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(4, danger);
    }

    [Fact]
    public void 数牌9_スジあり_危険度3を返す()
    {
        var tile = new Tile(TileKind.Man9.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Man6);

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(3, danger);
    }

    [Fact]
    public void 数牌9_スジなし_危険度6を返す()
    {
        var tile = new Tile(TileKind.Man9.Value * 4);
        var safeKinds = ImmutableHashSet<TileKind>.Empty;

        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        Assert.Equal(6, danger);
    }

    [Fact]
    public void スート境界をまたがない_4pがあっても1mは無スジ扱い()
    {
        // Arrange: 1m のスジは 4m のみ。safeKinds に 4p があっても無視される
        var tile = new Tile(TileKind.Man1.Value * 4);
        var safeKinds = ImmutableHashSet.Create(TileKind.Pin4);

        // Act
        var danger = DangerEvaluator.CalcDangerAgainst(tile, safeKinds, CreateView());

        // Assert
        Assert.Equal(6, danger);
    }

    private static PlayerRoundView CreateView(Hand? ownHand = null)
    {
        return new PlayerRoundView(
            ViewerIndex: new PlayerIndex(0),
            RoundWind: RoundWind.East,
            RoundNumber: new RoundNumber(0),
            Honba: new Honba(0),
            KyoutakuRiichiCount: new KyoutakuRiichiCount(0),
            TurnIndex: new PlayerIndex(0),
            PointArray: new PointArray(new Point(25000)),
            OwnHand: ownHand ?? new Hand(),
            CallListArray: new CallListArray(),
            RiverArray: new RiverArray(),
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses:
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ],
            WallLiveRemaining: 70
        );
    }
}

public class AI_v0_4_0_回し打ち_CalcDangerTests
{
    [Fact]
    public void リーチ者なし_危険度0を返す()
    {
        // Arrange
        var view = CreateView(riichiIndices: []);
        var tile = new Tile(TileKind.Man5.Value * 4);  // 無スジ = 12 相当

        // Act
        var danger = DangerEvaluator.CalcDanger(tile, view);

        // Assert: リーチ者がいないので 0
        Assert.Equal(0, danger);
    }

    [Fact]
    public void リーチ者1人_その相手の危険度を返す()
    {
        // Arrange: リーチ者 P1 の SafeKindsAgainstRiichi に 2m と 8m が含まれている → 5m は両スジ
        var safeKinds = ImmutableHashSet.Create(TileKind.Man2, TileKind.Man8);
        var view = CreateView(
            otherStatuses:
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), true, false, true, safeKinds),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ]
        );
        var tile = new Tile(TileKind.Man5.Value * 4);

        // Act
        var danger = DangerEvaluator.CalcDanger(tile, view);

        // Assert: 両スジなので 4
        Assert.Equal(4, danger);
    }

    [Fact]
    public void 複数リーチ者_最大値を返す()
    {
        // Arrange: P1 の safeKinds に 2m/8m (5m は両スジ = 危険度 4)、P2 の safeKinds は空 (5m は無スジ = 危険度 12)
        var p1Safe = ImmutableHashSet.Create(TileKind.Man2, TileKind.Man8);
        var p2Safe = ImmutableHashSet<TileKind>.Empty;
        var view = CreateView(
            otherStatuses:
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), true, false, true, p1Safe),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), true, false, true, p2Safe),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ]
        );
        var tile = new Tile(TileKind.Man5.Value * 4);

        // Act
        var danger = DangerEvaluator.CalcDanger(tile, view);

        // Assert: P2 視点では 5m は無スジ (12)、P1 視点では両スジ (4)。最大値は 12
        Assert.Equal(12, danger);
    }

    [Fact]
    public void SafeKindsAgainstRiichiがnullのリーチ者_無視される()
    {
        // Arrange: IsRiichi=true だが SafeKindsAgainstRiichi=null の異常ケース
        var view = CreateView(
            otherStatuses:
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), true, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ]
        );
        var tile = new Tile(TileKind.Man5.Value * 4);

        // Act
        var danger = DangerEvaluator.CalcDanger(tile, view);

        // Assert: SafeKinds がないので判定できず 0
        Assert.Equal(0, danger);
    }

    private static PlayerRoundView CreateView(
        PlayerIndex[]? riichiIndices = null,
        ImmutableArray<VisiblePlayerRoundStatus>? otherStatuses = null
    )
    {
        ImmutableArray<VisiblePlayerRoundStatus> statuses;
        if (otherStatuses.HasValue)
        {
            statuses = otherStatuses.Value;
        }
        else
        {
            var riichiSet = riichiIndices is null ? [] : new HashSet<int>(riichiIndices.Select(x => x.Value));
            statuses = [.. Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
                .Where(x => x != 0)
                .Select(x => new VisiblePlayerRoundStatus(
                    new PlayerIndex(x),
                    riichiSet.Contains(x),
                    false,
                    true,
                    riichiSet.Contains(x) ? [] : null
                ))
            ];
        }

        return new PlayerRoundView(
            ViewerIndex: new PlayerIndex(0),
            RoundWind: RoundWind.East,
            RoundNumber: new RoundNumber(0),
            Honba: new Honba(0),
            KyoutakuRiichiCount: new KyoutakuRiichiCount(0),
            TurnIndex: new PlayerIndex(0),
            PointArray: new PointArray(new Point(25000)),
            OwnHand: new Hand(),
            CallListArray: new CallListArray(),
            RiverArray: new RiverArray(),
            DoraIndicators: [],
            OwnStatus: new OwnRoundStatus(false, false, false, true, false, false, false, null),
            OtherPlayerStatuses: statuses,
            WallLiveRemaining: 70
        );
    }
}
