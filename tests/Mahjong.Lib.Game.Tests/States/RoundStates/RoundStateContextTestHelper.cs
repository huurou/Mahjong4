using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using Moq;
using System.Collections.Immutable;

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
            .Returns(new ScoreResult(0, 0, new PointArray(new Point(0)), []));
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
        mock.Setup(x => x.IsKoutsuOnlyInAllInterpretations(It.IsAny<Hand>(), It.IsAny<CallList>(), It.IsAny<int>()))
            .Returns(true);
        return mock.Object;
    }

    /// <summary>
    /// 既定の no-op サービスを注入した <see cref="RoundStateContext"/> を生成する。
    /// state 単体テスト用 (StartAsync は呼ばず、<see cref="InitDirect"/> + Drive* で同期駆動する想定)
    /// </summary>
    internal static RoundStateContext CreateContext()
    {
        return RoundStateContextRuntimeTestHelper.CreateDefaultContext(Games.GamesTestHelper.CreatePlayerList());
    }

    /// <summary>
    /// state 単体テスト用の同期初期化。Init() を介さず、Round/State を直接セットして Entry のみ呼ぶ。
    /// event queue (eventChannel_) は使われないため、後続の Drive* 系も完全同期で動作する
    /// </summary>
    internal static void InitDirect(RoundStateContext context, Round round)
    {
        context.Round = round.Haipai();
        context.State = new RoundStateHaipai();
        context.State.Entry(context);
    }

    internal static void DriveResponseOk(RoundStateContext context)
    {
        context.State.ResponseOk(context, new RoundEventResponseOk());
    }

    internal static void DriveResponseDahai(RoundStateContext context, Tile tile, bool isRiichi = false)
    {
        context.State.ResponseDahai(context, new RoundEventResponseDahai(tile, isRiichi));
    }

    internal static void DriveResponseKan(RoundStateContext context, CallType type, Tile tile)
    {
        context.State.ResponseKan(context, new RoundEventResponseKan(type, tile));
    }

    internal static void DriveResponseCall(
        RoundStateContext context,
        PlayerIndex callerIndex,
        CallType type,
        ImmutableList<Tile> handTiles,
        Tile calledTile
    )
    {
        context.State.ResponseCall(context, new RoundEventResponseCall(callerIndex, type, handTiles, calledTile));
    }

    internal static void DriveResponseWin(
        RoundStateContext context,
        ImmutableArray<PlayerIndex> winnerIndices,
        PlayerIndex loserIndex,
        WinType winType
    )
    {
        context.State.ResponseWin(context, new RoundEventResponseWin(winnerIndices, loserIndex, winType));
    }

    internal static void DriveResponseRyuukyoku(
        RoundStateContext context,
        RyuukyokuType type,
        ImmutableArray<PlayerIndex> tenpaiPlayers
    )
    {
        context.State.ResponseRyuukyoku(context, new RoundEventResponseRyuukyoku(type, tenpaiPlayers));
    }

    /// <summary>
    /// <see cref="Round.Turn"/> によるツモ和了応答を同期駆動で発行する
    /// </summary>
    internal static void DriveTsumoWin(RoundStateContext context)
    {
        DriveResponseWin(context, [context.Round.Turn], context.Round.Turn, WinType.Tsumo);
    }

    /// <summary>
    /// ロン和了応答を同期駆動で発行する
    /// </summary>
    internal static void DriveRonWin(RoundStateContext context, PlayerIndex winnerIndex, PlayerIndex loserIndex)
    {
        DriveResponseWin(context, [winnerIndex], loserIndex, WinType.Ron);
    }

    /// <summary>
    /// 槍槓和了応答を同期駆動で発行する
    /// </summary>
    internal static void DriveChankanWin(RoundStateContext context, PlayerIndex winnerIndex, PlayerIndex loserIndex)
    {
        DriveResponseWin(context, [winnerIndex], loserIndex, WinType.Chankan);
    }

    /// <summary>
    /// 嶺上開花和了応答を同期駆動で発行する (loser は和了者自身 = 現手番)
    /// </summary>
    internal static void DriveRinshanWin(RoundStateContext context)
    {
        DriveResponseWin(context, [context.Round.Turn], context.Round.Turn, WinType.Rinshan);
    }

    /// <summary>
    /// 荒牌平局・テンパイ者なしの流局応答を同期駆動で発行する
    /// </summary>
    internal static void DriveKouhaiHeikyoku(RoundStateContext context)
    {
        DriveResponseRyuukyoku(context, RyuukyokuType.KouhaiHeikyoku, []);
    }

    /// <summary>
    /// 和了/流局終端状態に到達済みであることを確認し、OK応答 (RoundEnded 発火) を同期駆動する
    /// </summary>
    internal static void DriveAcknowledgeRoundEnd<T>(RoundStateContext context) where T : RoundState
    {
        Assert.IsType<T>(context.State);
        DriveResponseOk(context);
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
