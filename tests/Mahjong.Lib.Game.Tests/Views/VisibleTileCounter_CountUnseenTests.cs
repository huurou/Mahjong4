using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;
using ScoringTileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Tests.Views;

public class VisibleTileCounter_CountUnseenTests
{
    [Fact]
    public void 全く見えていない_4を返す()
    {
        // Arrange
        var view = CreateView();

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[0]);

        // Assert
        Assert.Equal(4, count);
    }

    [Fact]
    public void 手牌に1枚_3を返す()
    {
        // Arrange
        var view = CreateView(ownHand: new Hand([Tile(0, 0)]));

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[0]);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void 他家の河に2枚_2を返す()
    {
        // Arrange
        var rivers = new RiverArray().AddTile(new PlayerIndex(1), Tile(5, 0)).AddTile(new PlayerIndex(1), Tile(5, 1));
        var view = CreateView(rivers: rivers);

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[5]);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void ポン副露3枚_1を返す()
    {
        // Arrange
        var call = new Call(
            CallType.Pon,
            ImmutableList.Create(Tile(10, 0), Tile(10, 1), Tile(10, 2)),
            new PlayerIndex(2),
            Tile(10, 0)
        );
        var calls = new CallListArray().AddCall(new PlayerIndex(0), call);
        var view = CreateView(calls: calls);

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[10]);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void 暗槓4枚_0を返す()
    {
        // Arrange
        var ankan = new Call(
            CallType.Ankan,
            ImmutableList.Create(Tile(33, 0), Tile(33, 1), Tile(33, 2), Tile(33, 3)),
            new PlayerIndex(0),
            null
        );
        var calls = new CallListArray().AddCall(new PlayerIndex(3), ankan);
        var view = CreateView(calls: calls);

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[33]);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void ドラ表示牌1枚_3を返す()
    {
        // Arrange
        var view = CreateView(doraIndicators: [Tile(27, 0)]);

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[27]);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void 合計5枚以上見えている_0にクランプされる()
    {
        // Arrange (手牌 2 + 河 2 + ドラ表示 1 = 5)
        var rivers = new RiverArray()
            .AddTile(new PlayerIndex(0), Tile(20, 0))
            .AddTile(new PlayerIndex(1), Tile(20, 1));
        var view = CreateView(
            ownHand: new Hand([Tile(20, 2), Tile(20, 3)]),
            rivers: rivers,
            doraIndicators: [Tile(20, 0)]
        );

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[20]);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void 複数ソースの合計_正しくカウントされる()
    {
        // Arrange (手牌 1 + 河 1 + 副露 1 = 3) → 残り 1
        var rivers = new RiverArray().AddTile(new PlayerIndex(2), Tile(15, 0));
        var call = new Call(
            CallType.Pon,
            ImmutableList.Create(Tile(15, 1), Tile(15, 2), Tile(15, 3)),
            new PlayerIndex(1),
            Tile(15, 1)
        );
        var calls = new CallListArray().AddCall(new PlayerIndex(3), call);
        var view = CreateView(
            ownHand: new Hand([Tile(15, 0)]),
            rivers: rivers,
            calls: calls
        );

        // Act
        var count = VisibleTileCounter.CountUnseen(view, ScoringTileKind.All[15]);

        // Assert
        Assert.Equal(0, count);
    }

    private static Tile Tile(int kind, int copy)
    {
        return new Tile(kind * 4 + copy);
    }

    private static PlayerRoundView CreateView(
        Hand? ownHand = null,
        CallListArray? calls = null,
        RiverArray? rivers = null,
        ImmutableList<Tile>? doraIndicators = null
    )
    {
        return new PlayerRoundView(
            new PlayerIndex(0),
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(35000)),
            ownHand ?? new Hand(),
            calls ?? new CallListArray(),
            rivers ?? new RiverArray(),
            doraIndicators ?? [],
            new OwnRoundStatus(false, false, false, true, false, false, false, null),
            [
                new VisiblePlayerRoundStatus(new PlayerIndex(1), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(2), false, false, true, null),
                new VisiblePlayerRoundStatus(new PlayerIndex(3), false, false, true, null),
            ],
            70
        );
    }
}
