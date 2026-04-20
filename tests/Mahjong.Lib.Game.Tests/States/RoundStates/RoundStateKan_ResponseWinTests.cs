using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateKan_ResponseWinTests : IDisposable
{
    private readonly RoundStateContext context_ = RoundStateContextTestHelper.CreateContext();

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void 暗槓中のChankan応答_受理されてRoundStateWinに遷移する()
    {
        // Arrange: 暗槓直後の RoundStateKan
        // 暗槓チャンカンは国士無双時のみ成立する。ScoringHelper が役なしエラーにならないよう
        // 親の暗槓 m1 + 子(index 1) の国士単騎テンパイ (m1待ち) を仕込む
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.InjectAnkanChankanScenario(context_, new PlayerIndex(1));
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));

        // Act
        RoundStateContextTestHelper.DriveChankanWin(context_, new PlayerIndex(1), new PlayerIndex(0));

        // Assert
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void 加槓が末尾でない場合でも槍槓が成立する()
    {
        // Arrange: 親に2つのポン (m1/m2) を持たせ、最初のポン (m1) を加槓で槓子化する。
        // 加槓追加牌が副露リスト末尾ではない場合でも KanTiles の末尾から winTile を取得することを検証する。
        // 子 index 1 は m1 単騎国士テンパイ → 加槓追加の m1 を槍槓ロンで役満成立
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);

        var dealer = context_.Round.Turn;
        var winner = new PlayerIndex(1);

        // 親手牌 = 加槓用の Tile(2) (m1) + ダミー 7 枚 = 8 枚 (ポン2副露ありで 14 - 3*2 = 8 枚)
        // 4 番目の m1 (Tile(2)) を加槓追加牌に使い、残り 3 枚 (Tile(0),(1),(3)) は pon1 に含める
        var dealerHand = new[]
        {
            new Tile(2),    // m1 (加槓で追加)
            new Tile(40), new Tile(44), new Tile(48),  // p234
            new Tile(78), new Tile(82), new Tile(86),  // s2 s3 s4
            new Tile(92),                                // s6
        };
        var handArray = context_.Round.HandArray;
        foreach (var t in handArray[dealer].ToList())
        {
            handArray = handArray.RemoveTile(dealer, t);
        }
        handArray = handArray.AddTiles(dealer, dealerHand);
        var pon1 = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(3)], new PlayerIndex(2), new Tile(3));
        var pon2 = new Call(CallType.Pon, [new Tile(4), new Tile(5), new Tile(7)], new PlayerIndex(2), new Tile(7));
        context_.Round = context_.Round with
        {
            HandArray = handArray,
            CallListArray = context_.Round.CallListArray
                .AddCall(dealer, pon1)
                .AddCall(dealer, pon2),
        };

        // 子 index 1: 国士 m1 単騎テンパイ (m1 を除く 12 種 + m9 重複で 13 枚)
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
        handArray = context_.Round.HandArray;
        foreach (var t in handArray[winner].ToList())
        {
            handArray = handArray.RemoveTile(winner, t);
        }
        handArray = handArray.AddTiles(winner, winnerTiles);
        context_.Round = context_.Round with { HandArray = handArray };

        // Kakan on first pon (kind 0) — replaces at index 0, not the end
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Kakan, new Tile(2));

        // Act: 槍槓
        RoundStateContextTestHelper.DriveChankanWin(context_, winner, dealer);

        // Assert: RoundStateWin に遷移
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void 過去に加槓済みで今回暗槓_Chankan応答は受理される()
    {
        // Arrange: 副露リストに過去の加槓 (m5 中張牌) を含むが、今回の操作は暗槓 (m9 幺九牌) にする
        // 子(index 1) は m9 単騎国士テンパイ。P1 fix 下で ScoringHelper が役満 (国士無双) を成立させる
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);

        var dealer = context_.Round.Turn;
        var winner = new PlayerIndex(1);
        // 親の手牌 = m9×4 (暗槓用) + ダミー 7 枚 (副露 1 つあるので 14 - 3 = 11 枚)
        var dealerTiles = new[]
        {
            new Tile(32), new Tile(33), new Tile(34), new Tile(35),   // m9 × 4 (暗槓用)
            new Tile(13), new Tile(41), new Tile(45),                  // ダミー m4 p2 p3
            new Tile(49), new Tile(77), new Tile(81), new Tile(85),    // ダミー p4 s2 s3 s4
        };
        var handArray = context_.Round.HandArray;
        foreach (var t in handArray[dealer].ToList())
        {
            handArray = handArray.RemoveTile(dealer, t);
        }
        handArray = handArray.AddTiles(dealer, dealerTiles);
        // 過去の加槓は中張牌 m5 (kind 4)。幺九牌ではないので 国士判定に影響しない
        var pastKakan = new Call(CallType.Kakan, [new Tile(16), new Tile(17), new Tile(18), new Tile(19)], dealer, new Tile(16));
        context_.Round = context_.Round with
        {
            HandArray = handArray,
            CallListArray = context_.Round.CallListArray.AddCall(dealer, pastKakan),
        };

        // 子 index 1: 国士 m9 単騎テンパイ (m9 を除く 12 種 + 1 重複で 13 枚)
        var winnerTiles = new[]
        {
            new Tile(0), new Tile(1),    // m1 m1 (雀頭候補)
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
        handArray = context_.Round.HandArray;
        foreach (var t in handArray[winner].ToList())
        {
            handArray = handArray.RemoveTile(winner, t);
        }
        handArray = handArray.AddTiles(winner, winnerTiles);
        context_.Round = context_.Round with { HandArray = handArray };

        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, new Tile(32));

        // Act
        RoundStateContextTestHelper.DriveChankanWin(context_, winner, dealer);

        // Assert: 暗槓 Chankan も受理して RoundStateWin へ遷移
        Assert.IsType<RoundStateWin>(context_.State);
    }

    [Fact]
    public void ツモ和了応答_例外で遷移しない()
    {
        // Arrange
        RoundStateContextTestHelper.InitDirect(context_, RoundStateContextTestHelper.CreateRound());
        RoundStateContextTestHelper.DriveResponseOk(context_);
        RoundStateContextTestHelper.DriveResponseKan(context_, CallType.Ankan, RoundStateContextTestHelper.PickAnkanTile(context_));

        // Act: Tsumo を渡す (槍槓状態に Tsumo は不正)
        var ex = Record.Exception(() => RoundStateContextTestHelper.DriveTsumoWin(context_));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<RoundStateKan>(context_.State);
    }
}
