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
/// 自動対局の統計集計を行う <see cref="IGameTracer"/>
/// </summary>
public sealed class StatsTracer : IGameTracer
{
    private const int PLAYER_COUNT = 4;

    private readonly int[,] rankCounts_ = new int[PLAYER_COUNT, PLAYER_COUNT];
    private readonly int[] winCounts_ = new int[PLAYER_COUNT];
    private readonly int[] houjuuCounts_ = new int[PLAYER_COUNT];
    private readonly int[] riichiCounts_ = new int[PLAYER_COUNT];
    private readonly int[] callCounts_ = new int[PLAYER_COUNT];
    private readonly long[] winPointSums_ = new long[PLAYER_COUNT];
    private readonly Dictionary<string, int> yakuCounts_ = [];
    private readonly Dictionary<RyuukyokuType, int> ryuukyokuCounts_ = [];
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
        if (notification is GameEndNotification gameEnd && recipientIndex.Value == 0)
        {
            RecordGameEnd(gameEnd.FinalPointArray);
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
        switch (adopted.Response)
        {
            case DahaiResponse d when d.IsRiichi && !riichiDeclaredThisRound_[playerIndex]:
                riichiDeclaredThisRound_[playerIndex] = true;
                riichiCounts_[playerIndex]++;
                break;

            case ChiResponse:
            case PonResponse:
            case DaiminkanResponse:
                if (!calledThisRound_[playerIndex])
                {
                    calledThisRound_[playerIndex] = true;
                    callCounts_[playerIndex]++;
                }
                break;
        }
    }

    public void OnRoundStarted(Round round)
    {
        roundCount_++;
        Array.Clear(riichiDeclaredThisRound_);
        Array.Clear(calledThisRound_);
    }

    public void OnRoundEnded(AdoptedRoundAction action)
    {
        switch (action)
        {
            case AdoptedWinAction win:
                foreach (var winner in win.WinnerIndices)
                {
                    winCounts_[winner.PlayerIndex.Value]++;
                    winPointSums_[winner.PlayerIndex.Value] += winner.ScoreResult.PointDeltas[winner.PlayerIndex].Value;
                    foreach (var yaku in winner.ScoreResult.YakuInfos)
                    {
                        yakuCounts_[yaku.Name] = yakuCounts_.GetValueOrDefault(yaku.Name) + 1;
                    }
                }
                if (win.WinType is WinType.Ron or WinType.Chankan)
                {
                    houjuuCounts_[win.LoserIndex.Value]++;
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
            rankCounts_[indexed[rank].Index, rank]++;
        }
    }

    public StatsReport Build()
    {
        var rankCountsBuilder = ImmutableArray.CreateBuilder<ImmutableArray<int>>(PLAYER_COUNT);
        for (var i = 0; i < PLAYER_COUNT; i++)
        {
            var row = ImmutableArray.CreateBuilder<int>(PLAYER_COUNT);
            for (var rank = 0; rank < PLAYER_COUNT; rank++)
            {
                row.Add(rankCounts_[i, rank]);
            }
            rankCountsBuilder.Add(row.ToImmutable());
        }

        return new StatsReport(
            gameCount_,
            roundCount_,
            rankCountsBuilder.ToImmutable(),
            [.. winCounts_],
            [.. houjuuCounts_],
            [.. riichiCounts_],
            [.. callCounts_],
            [.. winPointSums_],
            yakuCounts_.ToImmutableDictionary(),
            ryuukyokuCounts_.ToImmutableDictionary(),
            failedGameCount_
        );
    }
}
