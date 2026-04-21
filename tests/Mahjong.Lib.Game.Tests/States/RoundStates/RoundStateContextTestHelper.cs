using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

internal static class RoundStateContextTestHelper
{
    /// <summary>
    /// 既定の RoundStateContext を生成する。state 単体テスト用 (StartAsync は呼ばず、<see cref="InitDirect"/> + Drive* で同期駆動する想定)
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
    internal static ImmutableList<Tile> DummyChiHandTiles()
    {
        return [new Tile(84), new Tile(88)];
    }

    /// <summary>
    /// state テストで使う Chi に必要な2枚 (Tile(84), Tile(88)) を caller の手牌へ注入する。
    /// </summary>
    internal static void InjectChiHand(RoundStateContext context, PlayerIndex callerIndex)
    {
        context.Round = context.Round with
        {
            HandArray = context.Round.HandArray.AddTile(callerIndex, new Tile(84)).AddTile(callerIndex, new Tile(88))
        };
    }

    /// <summary>
    /// 指定プレイヤーの手牌を 14 枚の有効な和了形に差し替える (Tsumo/Rinshan 用)。
    /// 手牌: m234 m567 p234 s234 p55 (順子3+順子1+雀頭、全て中張牌) → 断么九 (1翻) が成立し <see cref="ScoringHelper"/> がエラーを返さない
    /// </summary>
    internal static void ReplaceHandForTsumoAgari(RoundStateContext context, PlayerIndex winnerIndex)
    {
        var tiles = new[]
        {
            new Tile(4), new Tile(8), new Tile(12),    // m234
            new Tile(16), new Tile(20), new Tile(24),  // m567
            new Tile(40), new Tile(44), new Tile(48),  // p234
            new Tile(76), new Tile(80), new Tile(84),  // s234
            new Tile(52), new Tile(53),                // p5 p5 雀頭
        };
        ReplaceHand(context, winnerIndex, tiles);
    }

    /// <summary>
    /// Ron 和了シナリオの手牌を注入する。
    /// 親(dealer)の手牌末尾を <see cref="Tile"/>(53) (p5) にして <see cref="PickTileToDahai"/> が p5 を返すようにし、
    /// <paramref name="winnerIndex"/> には p5 単騎テンパイの 13 枚手牌を注入する。
    /// 断么九 (1翻) が成立し <see cref="ScoringHelper"/> がエラーを返さない
    /// </summary>
    internal static void InjectRonAgariScenario(RoundStateContext context, PlayerIndex winnerIndex)
    {
        var dealer = context.Round.Turn;
        var dealerTiles = new[]
        {
            new Tile(0), new Tile(1), new Tile(5), new Tile(9),      // m1 m1 m2 m3 (任意のダミー)
            new Tile(37), new Tile(41), new Tile(45),                // p1 p2 p3 (任意)
            new Tile(73), new Tile(77), new Tile(81),                // s1 s2 s3 (任意)
            new Tile(108), new Tile(109), new Tile(124),             // 東東白 (任意)
            new Tile(53),                                             // p5 — 打牌される (14枚目)
        };
        ReplaceHand(context, dealer, dealerTiles);

        var winnerTiles = new[]
        {
            new Tile(6), new Tile(10), new Tile(14),    // m2 m3 m4
            new Tile(18), new Tile(22), new Tile(26),   // m5 m6 m7
            new Tile(42), new Tile(46), new Tile(50),   // p2 p3 p4
            new Tile(78), new Tile(82), new Tile(86),   // s2 s3 s4
            new Tile(52),                                // p5 単騎待ち
        };
        ReplaceHand(context, winnerIndex, winnerTiles);
    }

    /// <summary>
    /// Chankan (暗槓) 和了シナリオの手牌を注入する。
    /// 親(dealer)の手牌を m1×4 + 10 枚のダミーにして <see cref="PickAnkanTile"/> が m1 を返すようにし、
    /// <paramref name="winnerIndex"/> には 12 種の幺九牌 + m9 重複 (m1 待ち国士無双単騎テンパイ) の 13 枚手牌を注入する。
    /// 役満 (国士無双) が成立し <see cref="ScoringHelper"/> がエラーを返さない
    /// </summary>
    internal static void InjectAnkanChankanScenario(RoundStateContext context, PlayerIndex winnerIndex)
    {
        var dealer = context.Round.Turn;
        var dealerTiles = new[]
        {
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),       // m1 × 4 (暗槓用)
            new Tile(5), new Tile(9), new Tile(13),                   // m2 m3 m4 (ダミー)
            new Tile(17), new Tile(21),                                // m5 m6
            new Tile(41), new Tile(45), new Tile(49),                  // p2 p3 p4
            new Tile(77), new Tile(81),                                // s2 s3
        };
        ReplaceHand(context, dealer, dealerTiles);

        // 国士無双 単騎テンパイ (m1 待ち)。13 種のうち m1 を抜いて m9 を 2 枚で 13 枚にする
        var winnerTiles = new[]
        {
            new Tile(32), new Tile(33),  // m9 m9 (雀頭候補)
            new Tile(36),                 // p1
            new Tile(68),                 // p9
            new Tile(72),                 // s1
            new Tile(104),                // s9
            new Tile(108),                // 東
            new Tile(112),                // 南
            new Tile(116),                // 西
            new Tile(120),                // 北
            new Tile(124),                // 白
            new Tile(128),                // 發
            new Tile(132),                // 中
        };
        ReplaceHand(context, winnerIndex, winnerTiles);
    }

    /// <summary>
    /// Rinshan (嶺上開花) 和了シナリオの手牌を注入する。
    /// 親(dealer)の手牌を m5×4 (暗槓用) + 嶺上ツモ直後に和了する 10 枚で差し替える。
    /// 連番ウォールでは <see cref="Wall.DrawRinshan"/> の 1 手目は <see cref="Tile"/>(1) (= m1) を返すため、
    /// 嶺上ツモ後の 11 枚手牌は m123 + p234 + s234 + s5s5 の 3面子+雀頭となり、暗槓 m5 を含めて 4 面子 1 雀頭の和了形になる。
    /// 嶺上開花 (1翻) が成立し <see cref="ScoringHelper"/> がエラーを返さない
    /// </summary>
    internal static void InjectRinshanAgariScenario(RoundStateContext context)
    {
        var dealer = context.Round.Turn;
        var dealerTiles = new[]
        {
            new Tile(16), new Tile(17), new Tile(18), new Tile(19),   // m5 × 4 (暗槓用)
            new Tile(5), new Tile(9),                                  // m2 m3 (嶺上 m1 と組んで m1m2m3 面子)
            new Tile(41), new Tile(45), new Tile(49),                  // p2 p3 p4 面子
            new Tile(77), new Tile(81), new Tile(85),                  // s2 s3 s4 面子
            new Tile(88), new Tile(89),                                // s5 s5 雀頭
        };
        ReplaceHand(context, dealer, dealerTiles);
    }

    private static void ReplaceHand(RoundStateContext context, PlayerIndex index, IEnumerable<Tile> tiles)
    {
        var handArray = context.Round.HandArray;
        foreach (var t in handArray[index].ToList())
        {
            handArray = handArray.RemoveTile(index, t);
        }
        handArray = handArray.AddTiles(index, tiles);
        context.Round = context.Round with { HandArray = handArray };
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
