using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Tracing;

/// <summary>
/// 自動対局の統計集計を行う <see cref="IGameTracer"/>。
/// 混在対局・席シャッフル対応のため AI種別 (DisplayName) をキーに集計する
/// </summary>
public sealed class StatsTracer : IGameTracer
{
    private const int PLAYER_COUNT = 4;

    private sealed class Accumulator
    {
        public int[] RankCounts { get; } = new int[PLAYER_COUNT];
        public int GameSeatCount;
        public int RoundAppearance;
        public int WinCount;
        public int HoujuuCount;
        public int RiichiCount;
        public int CallCount;
        public long WinPointSum;
    }

    private readonly Dictionary<string, Accumulator> byName_ = [];
    private readonly Dictionary<RyuukyokuType, int> ryuukyokuCounts_ = [];
    private readonly Dictionary<string, int> yakuCounts_ = [];

    private readonly string[] currentNames_ = new string[PLAYER_COUNT];
    private readonly bool[] riichiDeclaredThisRound_ = new bool[PLAYER_COUNT];
    private readonly bool[] calledThisRound_ = new bool[PLAYER_COUNT];

    private int gameCount_;
    private int roundCount_;
    private int failedGameCount_;

    /// <summary>
    /// 例外で完走できなかった対局を 1 件として計上する。<see cref="AutoPlayRunner"/> の catch 節から呼ばれる
    /// </summary>
    public void RecordGameFailed()
    {
        failedGameCount_++;
    }

    public void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification)
    {
    }

    public void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification)
    {
        if (recipientIndex.Value != 0) { return; }

        switch (notification)
        {
            case GameStartNotification gameStart:
                for (var i = 0; i < PLAYER_COUNT; i++)
                {
                    currentNames_[i] = gameStart.PlayerList[new PlayerIndex(i)].DisplayName;
                }
                break;

            case GameEndNotification gameEnd:
                RecordGameEnd(gameEnd.FinalPointArray);
                break;
        }
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
        var playerIndex = adopted.PlayerIndex.Value;
        var name = currentNames_[playerIndex];
        if (name is null) { return; }

        switch (adopted.Response)
        {
            case DahaiResponse { IsRiichi: true }:
            case KanTsumoDahaiResponse { IsRiichi: true }:
                if (!riichiDeclaredThisRound_[playerIndex])
                {
                    riichiDeclaredThisRound_[playerIndex] = true;
                    GetOrCreate(name).RiichiCount++;
                }
                break;

            case ChiResponse:
            case PonResponse:
            case DaiminkanResponse:
                if (!calledThisRound_[playerIndex])
                {
                    calledThisRound_[playerIndex] = true;
                    GetOrCreate(name).CallCount++;
                }
                break;
        }
    }

    public void OnRoundStarted(Round round)
    {
        roundCount_++;
        Array.Clear(riichiDeclaredThisRound_);
        Array.Clear(calledThisRound_);
        for (var i = 0; i < PLAYER_COUNT; i++)
        {
            var name = currentNames_[i];
            if (name is null) { continue; }
            GetOrCreate(name).RoundAppearance++;
        }
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        switch (action)
        {
            case AdoptedWinAction win:
                foreach (var winner in win.WinnerIndices)
                {
                    var winnerName = currentNames_[winner.PlayerIndex.Value];
                    if (winnerName is null) { continue; }
                    var acc = GetOrCreate(winnerName);
                    acc.WinCount++;
                    acc.WinPointSum += winner.ScoreResult.PointDeltas[winner.PlayerIndex].Value;
                    foreach (var yaku in winner.ScoreResult.YakuInfos)
                    {
                        yakuCounts_[yaku.Name] = yakuCounts_.GetValueOrDefault(yaku.Name) + 1;
                    }
                }
                if (win.WinType is WinType.Ron or WinType.Chankan)
                {
                    var loserName = currentNames_[win.LoserIndex.Value];
                    if (loserName is not null)
                    {
                        GetOrCreate(loserName).HoujuuCount++;
                    }
                }
                break;

            case AdoptedRyuukyokuAction ryu:
                ryuukyokuCounts_[ryu.Type] = ryuukyokuCounts_.GetValueOrDefault(ryu.Type) + 1;
                break;
        }
    }

    private void RecordGameEnd(PointArray finalPoints)
    {
        gameCount_++;
        var indexed = Enumerable.Range(0, PLAYER_COUNT)
            .Select(x => (Index: x, Point: finalPoints[new PlayerIndex(x)].Value))
            .OrderByDescending(x => x.Point)
            .ToList();
        for (var rank = 0; rank < PLAYER_COUNT; rank++)
        {
            var idx = indexed[rank].Index;
            var name = currentNames_[idx];
            if (name is null) { continue; }
            var acc = GetOrCreate(name);
            acc.RankCounts[rank]++;
            acc.GameSeatCount++;
        }
    }

    private Accumulator GetOrCreate(string name)
    {
        if (!byName_.TryGetValue(name, out var acc))
        {
            acc = new Accumulator();
            byName_[name] = acc;
        }
        return acc;
    }

    public StatsReport Build()
    {
        var playerStatsBuilder = ImmutableArray.CreateBuilder<PlayerStats>(byName_.Count);
        foreach (var kv in byName_.OrderBy(x => x.Key))
        {
            var a = kv.Value;
            playerStatsBuilder.Add(
                new PlayerStats(
                    kv.Key,
                    [.. a.RankCounts],
                    a.GameSeatCount,
                    a.RoundAppearance,
                    a.WinCount,
                    a.HoujuuCount,
                    a.RiichiCount,
                    a.CallCount,
                    a.WinPointSum
                )
            );
        }

        return new StatsReport(
            gameCount_,
            roundCount_,
            playerStatsBuilder.ToImmutable(),
            yakuCounts_.ToImmutableDictionary(),
            ryuukyokuCounts_.ToImmutableDictionary(),
            failedGameCount_
        );
    }
}
