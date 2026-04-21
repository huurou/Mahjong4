using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// 天鳳 JSON 牌譜エディタ (tenhou.net/6/#json=) 互換形式で 1 対局 = 1 JSONL、各行 = 1 局の牌譜を書き出す
/// </summary>
/// <remarks>
/// <para>ctor で受け取った <see cref="TextWriter"/> に対して、局が終了する度に 1 局分の JSON を 1 行として書き出す。
/// ファイル open/close は呼び出し側の責務で、本クラスは writer の所有権を持たず <see cref="Dispose"/> で writer を閉じない</para>
/// <para>出力形式: <c>{"title":[...],"name":[...],"rule":{"disp":..., "aka":1},"log":[[...局データ 17 要素...]]}</c></para>
/// </remarks>
/// <param name="writer">1 対局分の JSONL を書き出す TextWriter。open/close は呼び出し側が管理する</param>
/// <param name="players">プレイヤー (index 0 が起家)</param>
/// <param name="rules">対局ルール</param>
/// <param name="title">牌譜エディタ上段/下段に表示される 2 行タイトル。省略時は空 2 行</param>
public sealed class TenhouJsonPaifuRecorder(TextWriter writer, PlayerList players, GameRules rules, IReadOnlyList<string>? title = null) : IGameTracer, IDisposable
{
    private const int PLAYER_COUNT = 4;
    private const int TSUMOGIRI = 60;

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IReadOnlyList<string> title_ = title ?? ["", ""];

    private RoundBuilder? current_;
    private bool disposed_;

    /// <summary>
    /// 牌譜 IO の失敗は後続統計の整合性を壊すため、<see cref="CompositeGameTracer"/> で例外を再 throw させる
    /// </summary>
    public bool IsCritical => true;

    public void OnRoundStarted(Round round)
    {
        var builder = new RoundBuilder
        {
            Kyoku = round.RoundWind.Value * 4 + round.RoundNumber.Value,
            Honba = round.Honba.Value,
            Kyoutaku = round.KyoutakuRiichiCount.Value
        };
        for (var i = 0; i < PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            builder.StartPoints[i] = round.PointArray[playerIndex].Value;
            builder.Haipai[i] = [.. round.HandArray[playerIndex].Select(x => TenhouTileNumber.Convert(x, rules))];
            builder.Incoming[i] = [];
            builder.Outgoing[i] = [];
        }
        builder.DoraIndicators.Add(TenhouTileNumber.Convert(round.Wall.GetDoraIndicator(0), rules));
        current_ = builder;
    }

    public void OnTsumoDrawn(PlayerIndex turn, Tile drawnTile, bool isRinshan)
    {
        if (current_ is null) { return; }
        current_.Incoming[turn.Value].Add(TenhouTileNumber.Convert(drawnTile, rules));
    }

    public void OnDoraRevealed(Tile newIndicator)
    {
        if (current_ is null) { return; }
        current_.DoraIndicators.Add(TenhouTileNumber.Convert(newIndicator, rules));
    }

    public void OnRiichiDeclared(PlayerIndex player, int step)
    {
        if (current_ is null) { return; }
        if (step == 1)
        {
            // 直後の打牌を "r..." 接頭辞化するフラグを立てる
            current_.PendingRiichiPlayer = player.Value;
        }
    }

    public void OnCallExecuted(PlayerIndex caller, Call call)
    {
        if (current_ is null) { return; }
        var str = TenhouMeldStringEncoder.Encode(call, caller, rules);
        // チー/ポン/大明槓は鳴いた側の incoming、暗槓/加槓は outgoing に配置する (天鳳仕様)
        if (call.Type is CallType.Chi or CallType.Pon or CallType.Daiminkan)
        {
            current_.Incoming[caller.Value].Add(str);
        }
        else
        {
            current_.Outgoing[caller.Value].Add(str);
        }
    }

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
        if (current_ is null) { return; }
        switch (adopted.Response)
        {
            case DahaiResponse dahai:
                AppendDahai(adopted.PlayerIndex, dahai.Tile, dahai.IsRiichi);
                break;

            case KanTsumoDahaiResponse kanDahai:
                AppendDahai(adopted.PlayerIndex, kanDahai.Tile, kanDahai.IsRiichi);
                break;
        }
    }

    private void AppendDahai(PlayerIndex player, Tile tile, bool isRiichi)
    {
        if (current_ is null) { return; }
        var tileNum = TenhouTileNumber.Convert(tile, rules);
        // ツモ切り判定: incoming 末尾の牌番号と一致する場合 60 として記録する
        var incoming = current_.Incoming[player.Value];
        var isTsumogiri = incoming.Count > 0 && incoming[^1] is int lastTsumo && lastTsumo == tileNum;
        var emitted = isTsumogiri ? TSUMOGIRI : tileNum;
        object entry = isRiichi ? $"r{emitted}" : emitted;
        current_.Outgoing[player.Value].Add(entry);
        if (current_.PendingRiichiPlayer == player.Value)
        {
            current_.PendingRiichiPlayer = null;
        }
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        if (current_ is null) { return; }
        switch (action)
        {
            case AdoptedWinAction win:
                foreach (var indicator in win.UraDoraIndicators)
                {
                    current_.UraIndicators.Add(TenhouTileNumber.Convert(indicator, rules));
                }
                current_.Result = BuildWinResult(win);
                break;

            case AdoptedRyuukyokuAction ryu:
                current_.Result = BuildRyuukyokuResult(ryu);
                break;
        }
        WriteLine(current_);
        current_ = null;
    }

    private static object[] BuildWinResult(AdoptedWinAction win)
    {
        var result = new List<object> { "和了" };
        foreach (var winner in win.WinnerIndices)
        {
            var delta = Enumerable.Range(0, PLAYER_COUNT)
                .Select(x => winner.ScoreResult.PointDeltas[new PlayerIndex(x)].Value)
                .ToArray();
            var points = winner.ScoreResult.PointDeltas[winner.PlayerIndex].Value;
            var isYakuman = winner.ScoreResult.YakuList.Any(x => x.IsYakuman);
            // ダブル役満 (HanClosed=26) は単独で 2 倍役満を表すため、役数でなく翻数/13 で倍率を算出する
            var yakumanHan = winner.ScoreResult.YakuList
                .Where(x => x.IsYakuman)
                .Sum(x => winner.ScoreResult.IsMenzen ? x.HanClosed : x.HanOpen);
            var yakumanMultiplier = yakumanHan / 13;
            var scoreText = TenhouScoreTextFormatter.Format(
                winner.ScoreResult.Han,
                winner.ScoreResult.Fu,
                points,
                isYakuman,
                yakumanMultiplier
            );
            // 包成立時は PaoPlayerIndex、未成立時は winner 自身を paoWho とする (天鳳仕様)
            var paoWho = winner.PaoPlayerIndex ?? winner.PlayerIndex;
            var detail = new List<object>
            {
                winner.PlayerIndex.Value,
                win.LoserIndex.Value,
                paoWho.Value,
                scoreText,
            };
            // ドラ/裏ドラ/赤ドラは YakuList に枚数分の重複要素として積まれるため、天鳳 JSON では同一役を 1 エントリに畳んで翻数を合算する
            // (重複のまま出力すると天鳳公式ビューワーで和了結果が "不明" 扱いになる)
            foreach (var group in winner.ScoreResult.YakuList.GroupBy(x => x))
            {
                var yaku = group.Key;
                if (yaku.IsYakuman)
                {
                    detail.Add($"{yaku.Name}(役満)");
                    continue;
                }
                var perYakuHan = winner.ScoreResult.IsMenzen ? yaku.HanClosed : yaku.HanOpen;
                var han = perYakuHan * group.Count();
                detail.Add($"{yaku.Name}({han}飜)");
            }
            result.Add(delta);
            result.Add(detail.ToArray());
        }
        return [.. result];
    }

    private static object[] BuildRyuukyokuResult(AdoptedRyuukyokuAction ryu)
    {
        var hasNagashi = ryu.NagashiManganPlayerIndices.Count > 0;
        var name = TenhouRyuukyokuNameMapper.ToName(ryu.Type, hasNagashi);
        if (!TenhouRyuukyokuNameMapper.HasPointDelta(ryu.Type, hasNagashi))
        {
            return [name];
        }
        var deltas = Enumerable.Range(0, PLAYER_COUNT)
            .Select(x => ryu.PointDeltas[new PlayerIndex(x)].Value)
            .ToArray();
        return [name, deltas];
    }

    private void WriteLine(RoundBuilder builder)
    {
        var log = new object[]
        {
            new object[] { builder.Kyoku, builder.Honba, builder.Kyoutaku },
            builder.StartPoints.Select(x => (object)x).ToArray(),
            builder.DoraIndicators.Select(x => (object)x).ToArray(),
            builder.UraIndicators.Select(x => (object)x).ToArray(),
            builder.Haipai[0].Select(x => (object)x).ToArray(),
            builder.Incoming[0].ToArray(),
            builder.Outgoing[0].ToArray(),
            builder.Haipai[1].Select(x => (object)x).ToArray(),
            builder.Incoming[1].ToArray(),
            builder.Outgoing[1].ToArray(),
            builder.Haipai[2].Select(x => (object)x).ToArray(),
            builder.Incoming[2].ToArray(),
            builder.Outgoing[2].ToArray(),
            builder.Haipai[3].Select(x => (object)x).ToArray(),
            builder.Incoming[3].ToArray(),
            builder.Outgoing[3].ToArray(),
            builder.Result ?? ["流局"],
        };
        var root = new Dictionary<string, object>
        {
            ["title"] = title_,
            ["name"] = Enumerable.Range(0, PLAYER_COUNT).Select(x => players[new PlayerIndex(x)].DisplayName).ToArray(),
            ["rule"] = new Dictionary<string, object>
            {
                ["disp"] = BuildRuleDisp(),
                ["aka"] = rules.RedDoraTiles.Count > 0 ? 1 : 0,
            },
            ["log"] = new object[] { log },
        };
        var json = JsonSerializer.Serialize(root, JsonOptions);
        writer.WriteLine(json);
        writer.Flush();
    }

    private string BuildRuleDisp()
    {
        var format = rules.Format switch
        {
            GameFormat.Tonpuu => "東",
            GameFormat.Tonnan => "南",
            GameFormat.SingleRound => "一",
            _ => "",
        };
        var aka = rules.RedDoraTiles.Count > 0 ? "赤" : "";
        var kuitan = rules.KuitanAllowed ? "喰" : "";
        return $"{format}{kuitan}{aka}".Length == 0 ? "自動対局" : $"自動対局{format}{kuitan}{aka}";
    }

    public void Dispose()
    {
        if (disposed_) { return; }
        disposed_ = true;
        // writer は外部所有のため close しない
    }

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification)
    {
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
    }

    public void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response)
    {
    }

    public void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex)
    {
    }

    public void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex)
    {
    }

    public void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates)
    {
    }

    private sealed class RoundBuilder
    {
        public int Kyoku { get; set; }
        public int Honba { get; set; }
        public int Kyoutaku { get; set; }
        public int[] StartPoints { get; } = new int[PLAYER_COUNT];
        public List<int> DoraIndicators { get; } = [];
        public List<int> UraIndicators { get; } = [];
        public List<int>[] Haipai { get; } = new List<int>[PLAYER_COUNT];
        public List<object>[] Incoming { get; } = new List<object>[PLAYER_COUNT];
        public List<object>[] Outgoing { get; } = new List<object>[PLAYER_COUNT];
        public int? PendingRiichiPlayer { get; set; }
        public object[]? Result { get; set; }
    }
}
