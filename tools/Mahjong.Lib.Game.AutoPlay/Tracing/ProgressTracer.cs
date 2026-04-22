using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.Tiles;
using Microsoft.Extensions.Logging;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// 1局終了ごとに結果と持ち点状況をログ出力する <see cref="IGameTracer"/>
/// </summary>
public sealed class ProgressTracer(int gameNumber, int gameCount, PlayerList playerList, ILogger logger) : IGameTracer
{
    private GameStateContext? context_;

    public void SetContext(GameStateContext context)
    {
        context_ = context;
    }

    private string NameOf(PlayerIndex index)
    {
        return $"P{index.Value}({playerList[index].DisplayName})";
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

    public void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted)
    {
    }

    public void OnRoundStarted(Round round)
    {
    }

    public void OnTsumoDrawn(PlayerIndex turn, Tile drawnTile, bool isRinshan)
    {
    }

    public void OnDoraRevealed(Tile newIndicator)
    {
    }

    public void OnRiichiDeclared(PlayerIndex player, int step)
    {
    }

    public void OnCallExecuted(PlayerIndex caller, Call call)
    {
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        var round = context_?.RoundStateContext?.Round;
        if (round is null) { return; }

        logger.LogInformation(
            "対局 {GameNumber}/{GameCount} {RoundLabel} {Result} {Ranking}",
            gameNumber + 1,
            gameCount,
            FormatRoundLabel(round),
            FormatResult(action),
            FormatRanking(round.PointArray)
        );
    }

    private static string FormatRoundLabel(Round round)
    {
        var wind = round.RoundWind.Value switch
        {
            0 => "東",
            1 => "南",
            2 => "西",
            3 => "北",
            _ => "?",
        };
        return $"{wind}{round.RoundNumber.Value + 1}局 {round.Honba.Value}本場";
    }

    private string FormatResult(AdoptedRoundAction action)
    {
        return action switch
        {
            AdoptedWinAction win => FormatWin(win),
            AdoptedRyuukyokuAction ryu => $"流局 ({ryu.Type})",
            _ => action.GetType().Name,
        };
    }

    private string FormatWin(AdoptedWinAction win)
    {
        var winners = win.WinnerIndices.Select(x =>
        {
            var gain = x.ScoreResult.PointDeltas[x.PlayerIndex].Value;
            var typeLabel = win.WinType switch
            {
                WinType.Tsumo => "ツモ",
                WinType.Ron => $"ロン({NameOf(win.LoserIndex)})",
                WinType.Chankan => $"槍槓({NameOf(win.LoserIndex)})",
                WinType.Rinshan => "嶺上",
                _ => "和了",
            };
            return $"{NameOf(x.PlayerIndex)} {typeLabel} +{gain}({x.ScoreResult.Han}翻{x.ScoreResult.Fu}符)";
        });
        return string.Join(", ", winners);
    }

    private string FormatRanking(PointArray points)
    {
        var ranked = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => (PlayerIndex: new PlayerIndex(x), Point: points[new PlayerIndex(x)].Value))
            .OrderByDescending(x => x.Point)
            .Select((x, rank) => $"{rank + 1}位 {NameOf(x.PlayerIndex)}={x.Point}");
        return $"[{string.Join(", ", ranked)}]";
    }
}
