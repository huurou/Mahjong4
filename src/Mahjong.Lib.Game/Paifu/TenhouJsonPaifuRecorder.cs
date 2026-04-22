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
                // Dealer index = Kyoku % 4 (Kyoku = RoundWind*4 + RoundNumber、RoundNumber 0-3 がそのまま親席)
                current_.Result = BuildWinResult(win, dealerIndex: current_.Kyoku % PLAYER_COUNT);
                break;

            case AdoptedRyuukyokuAction ryu:
                current_.Result = BuildRyuukyokuResult(ryu);
                break;
        }
        WriteLine(current_);
        current_ = null;
    }

    // 本場 1 本あたりのロン加算 (放銃者→和了者)
    private const int HONBA_BONUS_RON = 300;

    // 本場 1 本あたりのツモ加算 (支払者 1 人あたり)
    private const int HONBA_BONUS_TSUMO_EACH = 100;

    // 供託 1 本あたりの点数
    private const int KYOUTAKU_STICK_POINTS = 1000;

    private static object[] BuildWinResult(AdoptedWinAction win, int dealerIndex)
    {
        var result = new List<object> { "和了" };
        var isRon = win.WinType is WinType.Ron or WinType.Chankan;
        var honba = win.Honba.Value;
        var kyoutakuCount = win.KyoutakuRiichiAward.Count;
        for (var wi = 0; wi < win.WinnerIndices.Count; wi++)
        {
            var winner = win.WinnerIndices[wi];
            var isPrimary = wi == 0;
            var delta = new int[PLAYER_COUNT];
            for (var i = 0; i < PLAYER_COUNT; i++)
            {
                delta[i] = winner.ScoreResult.PointDeltas[new PlayerIndex(i)].Value;
            }
            // 天鳳仕様: 本場と供託は上家取りで先頭和了者のみ受取。
            // Round.SettleWin は「各和了者が個別に本場受取」だが、paifu は tenhou.net/6 ビューワー互換優先で先頭集約する
            if (isPrimary)
            {
                ApplyHonbaBonus(delta, winner.PlayerIndex, win.LoserIndex, honba, isRon);
                if (kyoutakuCount > 0)
                {
                    delta[winner.PlayerIndex.Value] += KYOUTAKU_STICK_POINTS * kyoutakuCount;
                }
            }
            var isYakuman = winner.ScoreResult.YakuList.Any(x => x.IsYakuman);
            // ダブル役満 (HanClosed=26) は単独で 2 倍役満を表すため、役数でなく翻数/13 で倍率を算出する
            var yakumanHan = winner.ScoreResult.YakuList
                .Where(x => x.IsYakuman)
                .Sum(x => winner.ScoreResult.IsMenzen ? x.HanClosed : x.HanOpen);
            var yakumanMultiplier = yakumanHan / 13;
            var shape = BuildPaymentShape(win.WinType, winner, dealerIndex);
            var scoreText = TenhouScoreTextFormatter.Format(
                winner.ScoreResult.Han,
                winner.ScoreResult.Fu,
                shape,
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
            result.Add(delta.Cast<object>().ToArray());
            result.Add(detail.ToArray());
        }
        return [.. result];
    }

    private static void ApplyHonbaBonus(int[] delta, PlayerIndex winnerIndex, PlayerIndex loserIndex, int honba, bool isRon)
    {
        if (honba <= 0) { return; }
        if (isRon)
        {
            var bonus = HONBA_BONUS_RON * honba;
            delta[winnerIndex.Value] += bonus;
            delta[loserIndex.Value] -= bonus;
        }
        else
        {
            // Tsumo: 和了者が +300×honba、各非和了者が -100×honba
            var each = HONBA_BONUS_TSUMO_EACH * honba;
            for (var i = 0; i < PLAYER_COUNT; i++)
            {
                if (i == winnerIndex.Value)
                {
                    delta[i] += each * (PLAYER_COUNT - 1);
                }
                else
                {
                    delta[i] -= each;
                }
            }
        }
    }

    private static ScorePaymentShape BuildPaymentShape(WinType winType, AdoptedWinner winner, int dealerIndex)
    {
        var deltas = winner.ScoreResult.PointDeltas;
        var winnerIndex = winner.PlayerIndex;
        if (winType is WinType.Ron or WinType.Chankan)
        {
            return new RonPaymentShape(deltas[winnerIndex].Value);
        }
        // Tsumo/Rinshan
        if (winnerIndex.Value == dealerIndex)
        {
            // 親ツモ: 子 3 人が同額支払。いずれかの非和了席の delta から取得 (通常局面はすべて同額)
            var nonWinner = FirstIndex(x => x.Value != winnerIndex.Value);
            return new DealerTsumoPaymentShape(-deltas[nonWinner].Value);
        }
        // 子ツモ: 親支払と子支払を分けて取得
        var dealerPlayerIndex = new PlayerIndex(dealerIndex);
        var nonDealerNonWinner = FirstIndex(x =>
            x.Value != winnerIndex.Value &&
            x.Value != dealerIndex);
        return new ChildTsumoPaymentShape(
            NonDealerPay: -deltas[nonDealerNonWinner].Value,
            DealerPay: -deltas[dealerPlayerIndex].Value
        );
    }

    private static PlayerIndex FirstIndex(Func<PlayerIndex, bool> predicate)
    {
        for (var i = 0; i < PLAYER_COUNT; i++)
        {
            var p = new PlayerIndex(i);
            if (predicate(p)) { return p; }
        }
        throw new InvalidOperationException("条件を満たす PlayerIndex が見つかりません。");
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
