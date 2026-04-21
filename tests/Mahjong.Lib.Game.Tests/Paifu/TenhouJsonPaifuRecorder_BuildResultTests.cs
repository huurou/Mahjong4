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
