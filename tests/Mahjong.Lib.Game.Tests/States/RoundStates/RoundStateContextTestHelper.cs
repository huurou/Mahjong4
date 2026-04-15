using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

internal static class RoundStateContextTestHelper
{
    internal static Round CreateRound()
    {
        var wallGeneratorMock = new Mock<IWallGenerator>();
        wallGeneratorMock
            .Setup(x => x.Generate())
            .Returns(new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x))));
        return new Round(
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(25000)),
            wallGeneratorMock.Object);
    }

    /// <summary>
    /// 現手番プレイヤーの手牌からツモ直後の最終1枚を返す (打牌用)
    /// </summary>
    /// <remarks>
    /// CreateRound で生成される連番ウォールでは、親の第1ツモは yama[83] = Tile(83) (kind 20 = 3索)。
    /// 下記の DummyChiHandTiles と組み合わせて有効なチー (3-4-5索) を成立させるために Last() を用いる。
    /// </remarks>
    internal static Tile PickTileToDahai(RoundStateContext context)
    {
        return context.Round.HandArray[context.Round.Turn].Last();
    }

    /// <summary>
    /// 現手番プレイヤーの手牌から同種4枚がある牌を1枚返す (暗槓用)
    /// </summary>
    internal static Tile PickAnkanTile(RoundStateContext context)
    {
        var hand = context.Round.HandArray[context.Round.Turn];
        return hand.GroupBy(x => x.Id / 4).First(x => x.Count() >= 4).First();
    }

    /// <summary>
    /// チー用の有効な手牌2枚を返す。
    /// </summary>
    /// <remarks>
    /// PickTileToDahai が返す Tile(83) (kind 20 = 3索) と合わせて 3-4-5索 の順子を作る。
    /// </remarks>
    internal static ImmutableList<Tile> DummyChiHandTiles()
    {
        return [new Tile(84), new Tile(88)];
    }

    /// <summary>
    /// state テストで使う Chi に必要な2枚 (Tile(84), Tile(88)) を caller の手牌へ注入する。
    /// 連番ウォールの配牌では caller がこれらを持たないため、Chi 検証を通すための準備。
    /// </summary>
    internal static void InjectChiHand(RoundStateContext context, PlayerIndex caller)
    {
        context.Round = context.Round with
        {
            HandArray = context.Round.HandArray.AddTile(caller, new Tile(84)).AddTile(caller, new Tile(88))
        };
    }

    internal static async Task WaitForStateAsync<T>(
        RoundStateContext context,
        TimeSpan? timeout = null
    ) where T : RoundState
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? sender, RoundStateChangedEventArgs e)
        {
            if (e.State is T)
            {
                tcs.TrySetResult();
            }
        }

        context.RoundStateChanged += Handler;
        try
        {
            if (context.State is T)
            {
                return;
            }

            await tcs.Task.WaitAsync(timeout.Value);
        }
        finally
        {
            context.RoundStateChanged -= Handler;
        }
    }
}
