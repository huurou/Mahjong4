using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Yakus;
using System.Text.Json;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouJsonPaifuRecorder_BuildResultTests
{
    private static PlayerList CreatePlayers()
    {
        return new PlayerList([
            FakePlayer.Create(0),
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        ]);
    }

    private static (TenhouJsonPaifuRecorder Recorder, StringWriter Writer) CreateRecorder(GameRules rules)
    {
        var writer = new StringWriter();
        var recorder = new TenhouJsonPaifuRecorder(writer, CreatePlayers(), rules);
        return (recorder, writer);
    }

    private static JsonElement[] Emit(TenhouJsonPaifuRecorder recorder, StringWriter writer, Round round, AdoptedRoundAction action)
    {
        recorder.OnRoundStarted(round);
        recorder.OnRoundEnded(action);
        var line = writer.ToString().TrimEnd();
        using var doc = JsonDocument.Parse(line);
        // JsonElement.Clone で独立インスタンスを返し、JsonDocument Dispose 後でも参照可能にする
        return [.. doc.RootElement.GetProperty("log")[0][16].EnumerateArray().Select(x => x.Clone())];
    }

    [Fact]
    public void 包成立時_paoWhoに責任者が出力される()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(0, 0,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 48000)
                    .SubtractPoint(new PlayerIndex(2), 48000),
                [Yaku.Daisangen],
                IsMenzen: true),
            PaoPlayerIndex: new PlayerIndex(2)
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: result = ["和了", [deltas], [winner_who, loser, paoWho, scoreText, ...]]
        var detail = resultArr[2];
        Assert.Equal(0, detail[0].GetInt32()); // who
        Assert.Equal(0, detail[1].GetInt32()); // loser (ツモは winner 自身)
        Assert.Equal(2, detail[2].GetInt32()); // paoWho
    }

    [Fact]
    public void 包なしの和了_paoWhoはwinner自身()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(2, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 2000)
                    .SubtractPoint(new PlayerIndex(0), 2000),
                [],
                IsMenzen: false)
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var detail = resultArr[2];
        Assert.Equal(1, detail[0].GetInt32()); // who
        Assert.Equal(0, detail[1].GetInt32()); // loser
        Assert.Equal(1, detail[2].GetInt32()); // paoWho = winner 自身
    }

    [Fact]
    public void ダブル役満の和了_scoreTextは二倍役満を含む()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(0, 0,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 64000)
                    .SubtractPoint(new PlayerIndex(1), 32000)
                    .SubtractPoint(new PlayerIndex(2), 16000)
                    .SubtractPoint(new PlayerIndex(3), 16000),
                [Yaku.DaisuushiiDouble],
                IsMenzen: true)
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: detail[3] が scoreText
        var scoreText = resultArr[2][3].GetString();
        Assert.Contains("二倍役満", scoreText);
    }

    [Fact]
    public void 立直和了_裏ドラ表示牌がlog3に記録される()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        // 赤ドラ判定を避けるため 1m (kind 0) を裏ドラ表示牌として使う
        var uraIndicator = new Tile(0);
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(1, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 1500)
                    .SubtractPoint(new PlayerIndex(1), 500)
                    .SubtractPoint(new PlayerIndex(2), 500)
                    .SubtractPoint(new PlayerIndex(3), 500),
                [Yaku.Riichi],
                IsMenzen: true)
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [uraIndicator],
            false
        );

        // Act
        recorder.OnRoundStarted(round);
        recorder.OnRoundEnded(action);
        using var doc = JsonDocument.Parse(writer.ToString().TrimEnd());
        var roundData = doc.RootElement.GetProperty("log")[0];
        var ura = roundData[3];

        // Assert: 裏ドラは log[3] の位置。1m (kind 0) → 天鳳番号 11
        Assert.Equal(1, ura.GetArrayLength());
        Assert.Equal(11, ura[0].GetInt32());
    }

    [Fact]
    public void 裏ドラが複数枚_1エントリに畳まれて翻数が合算される()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                6, 25,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 12000)
                    .SubtractPoint(new PlayerIndex(1), 12000),
                [Yaku.Riichi, Yaku.Ippatsu, Yaku.Chiitoitsu, Yaku.Uradora, Yaku.Uradora],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(1),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: detail[4..] が役エントリ。裏ドラは 1 エントリに畳まれて 2 飜扱いになる
        var detail = resultArr[2];
        var yakuEntries = Enumerable.Range(4, detail.GetArrayLength() - 4)
            .Select(x => detail[x].GetString() ?? "")
            .ToArray();
        Assert.Equal(
            ["立直(1飜)", "一発(1飜)", "七対子(2飜)", "裏ドラ(2飜)"],
            yakuEntries
        );
    }

    [Fact]
    public void 流局_PointDeltasが出力される()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var pointDeltas = new PointArray(new Point(0))
            .AddPoint(new PlayerIndex(0), 3000)
            .SubtractPoint(new PlayerIndex(1), 1000)
            .SubtractPoint(new PlayerIndex(2), 1000)
            .SubtractPoint(new PlayerIndex(3), 1000);
        var action = new AdoptedRyuukyokuAction(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0)],
            [],
            pointDeltas,
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: 流局の結果は [name, deltas]
        Assert.Equal("流局", resultArr[0].GetString());
        var deltas = resultArr[1];
        Assert.Equal(3000, deltas[0].GetInt32());
        Assert.Equal(-1000, deltas[1].GetInt32());
        Assert.Equal(-1000, deltas[2].GetInt32());
        Assert.Equal(-1000, deltas[3].GetInt32());
    }

    [Fact]
    public void 本場1のロン_deltaに放銃者から和了者へ300点加算()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(
                2, 40,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 2600)
                    .SubtractPoint(new PlayerIndex(0), 2600),
                [],
                IsMenzen: false
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 0),
            new Honba(1),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: delta は素点 +/- 本場 300
        var delta = resultArr[1];
        Assert.Equal(-2600 - 300, delta[0].GetInt32());
        Assert.Equal(2600 + 300, delta[1].GetInt32());
        Assert.Equal(0, delta[2].GetInt32());
        Assert.Equal(0, delta[3].GetInt32());
    }

    [Fact]
    public void 本場2のツモ_各非和了者から100点ずつ和了者へ()
    {
        // Arrange: 親ツモを想定 (dealer=0, winner=0)
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                3, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 6000)
                    .SubtractPoint(new PlayerIndex(1), 2000)
                    .SubtractPoint(new PlayerIndex(2), 2000)
                    .SubtractPoint(new PlayerIndex(3), 2000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(2),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: winner には 100*2*3=600 加算、各非和了者から 100*2=200 減算
        var delta = resultArr[1];
        Assert.Equal(6000 + 600, delta[0].GetInt32());
        Assert.Equal(-2000 - 200, delta[1].GetInt32());
        Assert.Equal(-2000 - 200, delta[2].GetInt32());
        Assert.Equal(-2000 - 200, delta[3].GetInt32());
    }

    [Fact]
    public void 供託1本のロン_winner先頭に1000点加算()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(2),
            new Tile(0),
            new ScoreResult(
                3, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(2), 3900)
                    .SubtractPoint(new PlayerIndex(1), 3900),
                [],
                IsMenzen: false
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(1),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(2), 1),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: winner に +1000 供託、他は素点のまま
        var delta = resultArr[1];
        Assert.Equal(0, delta[0].GetInt32());
        Assert.Equal(-3900, delta[1].GetInt32());
        Assert.Equal(3900 + 1000, delta[2].GetInt32());
        Assert.Equal(0, delta[3].GetInt32());
    }

    [Fact]
    public void 局内リーチのみで開始kyoutakuゼロのロン_winnerに自分のリーチ棒1000点が加算される()
    {
        // Arrange: ヘッダ kyoutaku=0 だが局内リーチで SettleWin 時点 Count=1 になるケース (東1局の再現)
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(
                2, 40,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 2600)
                    .SubtractPoint(new PlayerIndex(0), 2600),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 1),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: winner (player 1) に +1000 加算されていること
        var delta = resultArr[1];
        Assert.Equal(-2600, delta[0].GetInt32());
        Assert.Equal(2600 + 1000, delta[1].GetInt32());
        Assert.Equal(0, delta[2].GetInt32());
        Assert.Equal(0, delta[3].GetInt32());
    }

    [Fact]
    public void 本場3供託2のツモ_delta合計がkyoutaku分だけプラス()
    {
        // Arrange: 親ツモ (dealer=0, winner=0)
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                3, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 6000)
                    .SubtractPoint(new PlayerIndex(1), 2000)
                    .SubtractPoint(new PlayerIndex(2), 2000)
                    .SubtractPoint(new PlayerIndex(3), 2000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 2),
            new Honba(3),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: delta 合計 = 供託分 (=2000)。winner に本場+供託、各子に本場マイナス
        var delta = resultArr[1];
        var sum = delta[0].GetInt32() + delta[1].GetInt32() + delta[2].GetInt32() + delta[3].GetInt32();
        Assert.Equal(2000, sum);
        Assert.Equal(6000 + 100 * 3 * 3 + 2000, delta[0].GetInt32());
        Assert.Equal(-2000 - 100 * 3, delta[1].GetInt32());
    }

    [Fact]
    public void ダブロン本場あり_本場と供託は上家取り先頭のみに加算()
    {
        // Arrange: winners[0]=index 1, winners[1]=index 2 が index 3 からロン
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner1 = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(
                2, 40,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 2600)
                    .SubtractPoint(new PlayerIndex(3), 2600),
                [],
                IsMenzen: false
            )
        );
        var winner2 = new AdoptedWinner(
            new PlayerIndex(2),
            new Tile(0),
            new ScoreResult(
                3, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(2), 3900)
                    .SubtractPoint(new PlayerIndex(3), 3900),
                [],
                IsMenzen: false
            )
        );
        var action = new AdoptedWinAction(
            [winner1, winner2],
            new PlayerIndex(3),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 1),
            new Honba(2),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: result = ["和了", [delta1], [detail1], [delta2], [detail2]]
        var delta1 = resultArr[1];
        // winner1 (上家取り先頭) に本場 600 + 供託 1000 加算、loser から本場 600 減算
        Assert.Equal(2600 + 600 + 1000, delta1[1].GetInt32());
        Assert.Equal(-2600 - 600, delta1[3].GetInt32());
        Assert.Equal(0, delta1[0].GetInt32());
        Assert.Equal(0, delta1[2].GetInt32());

        var delta2 = resultArr[3];
        // winner2 (2 人目) は素点のまま。本場・供託を載せない
        Assert.Equal(3900, delta2[2].GetInt32());
        Assert.Equal(-3900, delta2[3].GetInt32());
        Assert.Equal(0, delta2[0].GetInt32());
        Assert.Equal(0, delta2[1].GetInt32());
    }

    [Fact]
    public void 子ツモ_scoreTextは非親支払_親支払点形式()
    {
        // Arrange: 東2局 (dealer=1)、winner=index 0 の子ツモ 30符4翻
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound(roundNumber: 1).Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                4, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 7900)
                    .SubtractPoint(new PlayerIndex(1), 3900)
                    .SubtractPoint(new PlayerIndex(2), 2000)
                    .SubtractPoint(new PlayerIndex(3), 2000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var scoreText = resultArr[2][3].GetString();
        Assert.Equal("30符4飜2000-3900点", scoreText);
    }

    [Fact]
    public void 親ツモ_scoreTextは子支払点の全員支払形式()
    {
        // Arrange: 東1局 (dealer=0)、winner=index 0 の親ツモ 30符3翻
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                3, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 6000)
                    .SubtractPoint(new PlayerIndex(1), 2000)
                    .SubtractPoint(new PlayerIndex(2), 2000)
                    .SubtractPoint(new PlayerIndex(3), 2000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var scoreText = resultArr[2][3].GetString();
        Assert.Equal("30符3飜2000点∀", scoreText);
    }

    [Fact]
    public void 子ツモ満貫_scoreTextは満貫非親支払_親支払点形式()
    {
        // Arrange: 東2局、winner=index 0 の子ツモ満貫
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound(roundNumber: 1).Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                5, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 8000)
                    .SubtractPoint(new PlayerIndex(1), 4000)
                    .SubtractPoint(new PlayerIndex(2), 2000)
                    .SubtractPoint(new PlayerIndex(3), 2000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var scoreText = resultArr[2][3].GetString();
        Assert.Equal("満貫2000-4000点", scoreText);
    }

    [Fact]
    public void 親ツモ満貫_scoreTextは満貫子支払点の全員支払形式()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(0),
            new Tile(0),
            new ScoreResult(
                5, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(0), 12000)
                    .SubtractPoint(new PlayerIndex(1), 4000)
                    .SubtractPoint(new PlayerIndex(2), 4000)
                    .SubtractPoint(new PlayerIndex(3), 4000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(0),
            WinType.Tsumo,
            new KyoutakuRiichiAward(new PlayerIndex(0), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var scoreText = resultArr[2][3].GetString();
        Assert.Equal("満貫4000点∀", scoreText);
    }

    [Fact]
    public void 子ロン満貫_scoreTextは満貫点形式()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var winner = new AdoptedWinner(
            new PlayerIndex(1),
            new Tile(0),
            new ScoreResult(
                5, 30,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 8000)
                    .SubtractPoint(new PlayerIndex(2), 8000),
                [],
                IsMenzen: true
            )
        );
        var action = new AdoptedWinAction(
            [winner],
            new PlayerIndex(2),
            WinType.Ron,
            new KyoutakuRiichiAward(new PlayerIndex(1), 0),
            new Honba(0),
            [],
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert
        var scoreText = resultArr[2][3].GetString();
        Assert.Equal("満貫8000点", scoreText);
    }

    [Fact]
    public void 途中流局_deltasを含まない()
    {
        // Arrange
        var rules = new GameRules();
        var (recorder, writer) = CreateRecorder(rules);
        var round = RoundTestHelper.CreateRound().Haipai();
        var action = new AdoptedRyuukyokuAction(
            RyuukyokuType.SuuchaRiichi,
            [],
            [],
            new PointArray(new Point(0)),
            false
        );

        // Act
        var resultArr = Emit(recorder, writer, round, action);

        // Assert: 途中流局は [name] のみ (delta なし)
        Assert.Single(resultArr);
        Assert.Equal("四家立直", resultArr[0].GetString());
    }
}
