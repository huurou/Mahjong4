using System.Collections.Immutable;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

internal static class RoundStateContextTestHelper
{
    /// <summary>
    /// 点数計算を行わない既定の <see cref="IScoreCalculator"/> (全員 0 デルタ) を返す
    /// </summary>
    internal static IScoreCalculator CreateNoOpScoreCalculator()
    {
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(new ScoreResult(0, 0, new PointArray(new Point(0))));
        return mock.Object;
    }

    /// <summary>
    /// テンパイ判定を行わない既定の <see cref="ITenpaiChecker"/> (常に false / 空集合) を返す
    /// </summary>
    internal static ITenpaiChecker CreateNoOpTenpaiChecker()
    {
        var mock = new Mock<ITenpaiChecker>();
        mock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(false);
        mock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([]);
        return mock.Object;
    }

    /// <summary>
    /// 既定の no-op サービスを注入した <see cref="RoundStateContext"/> を生成する
    /// </summary>
    internal static RoundStateContext CreateContext()
    {
        return new RoundStateContext(CreateNoOpTenpaiChecker(), CreateNoOpScoreCalculator());
    }

    /// <summary>
    /// <see cref="Round.Turn"/> によるツモ和了応答をテスト用ショートカットとして発行する
    /// </summary>
    internal static Task ResponseTsumoWinAsync(RoundStateContext context)
    {
        return context.ResponseWinAsync([context.Round.Turn], context.Round.Turn, WinType.Tsumo);
    }

    /// <summary>
    /// ロン和了応答をテスト用ショートカットとして発行する
    /// </summary>
    internal static Task ResponseRonWinAsync(RoundStateContext context, PlayerIndex winnerIndex, PlayerIndex loserIndex)
    {
        return context.ResponseWinAsync([winnerIndex], loserIndex, WinType.Ron);
    }

    /// <summary>
    /// 槍槓和了応答をテスト用ショートカットとして発行する
    /// </summary>
    internal static Task ResponseChankanWinAsync(RoundStateContext context, PlayerIndex winnerIndex, PlayerIndex loserIndex)
    {
        return context.ResponseWinAsync([winnerIndex], loserIndex, WinType.Chankan);
    }

    /// <summary>
    /// 嶺上開花和了応答をテスト用ショートカットとして発行する (loser は和了者自身 = 現手番)
    /// </summary>
    internal static Task ResponseRinshanWinAsync(RoundStateContext context)
    {
        return context.ResponseWinAsync([context.Round.Turn], context.Round.Turn, WinType.Rinshan);
    }

    /// <summary>
    /// 荒牌平局・テンパイ者なしの流局応答をテスト用ショートカットとして発行する
    /// </summary>
    internal static Task ResponseKouhaiHeikyokuAsync(RoundStateContext context)
    {
        return context.ResponseRyuukyokuAsync(RyuukyokuType.KouhaiHeikyoku, []);
    }

    /// <summary>
    /// 和了/流局終端状態への到達を待機し、全プレイヤーOK応答相当の ResponseOk を発行して RoundEnded を進める
    /// </summary>
    internal static async Task AcknowledgeRoundEndAsync<T>(RoundStateContext context) where T : RoundState
    {
        await WaitForStateAsync<T>(context);
        await context.ResponseOkAsync();
    }

    internal static Round CreateRound()
    {
        return new Round(
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PlayerIndex(0),
            new PointArray(new Point(25000)),
            new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x))));
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
    internal static void InjectChiHand(RoundStateContext context, PlayerIndex callerIndex)
    {
        context.Round = context.Round with
        {
            HandArray = context.Round.HandArray.AddTile(callerIndex, new Tile(84)).AddTile(callerIndex, new Tile(88))
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
