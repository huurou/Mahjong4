using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 既定の通知ビルダー実装。
/// 問い合わせフェーズ (Haipai/Tsumo/Dahai/Kan/KanTsumo/AfterKanTsumo) と
/// 観測フェーズ (Call/Win/Ryuukyoku) の通知を状態に応じて振り分ける
/// </summary>
public sealed class RoundNotificationBuilder : IRoundNotificationBuilder
{
    public RoundNotification Build(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        PlayerInquirySpec playerSpec,
        IRoundViewProjector projector
    )
    {
        var view = projector.Project(round, playerSpec.PlayerIndex);
        var inquired = spec.InquiredPlayerIndices;
        var isInquired = spec.IsInquired(playerSpec.PlayerIndex);
        return state switch
        {
            RoundStateHaipai => new HaipaiNotification(view, inquired),
            RoundStateTsumo => isInquired
                // 問い合わせ対象 (手番): 自身のツモ牌を含む TsumoNotification
                ? new TsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                // 非対象 (他家): ツモ牌は私的情報のため送らない OtherPlayerTsumoNotification
                : new OtherPlayerTsumoNotification(view, round.Turn, inquired),
            RoundStateDahai => BuildDahaiNotification(view, round, playerSpec.CandidateList, inquired),
            RoundStateKan kan => BuildKanNotification(view, round, kan, playerSpec.CandidateList, inquired),
            RoundStateKanTsumo => isInquired
                ? new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                : new OtherPlayerKanTsumoNotification(view, round.Turn, inquired),
            RoundStateAfterKanTsumo => isInquired
                ? new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                : new OtherPlayerKanTsumoNotification(view, round.Turn, inquired),
            RoundStateCall => BuildCallNotification(view, round, inquired),
            RoundStateWin win => new WinNotification(view, (AdoptedWinAction)AdoptedRoundActionBuilder.Build(win.EventArgs), inquired),
            RoundStateRyuukyoku ryu => new RyuukyokuNotification(view, (AdoptedRyuukyokuAction)AdoptedRoundActionBuilder.Build(ryu.EventArgs), inquired),
            _ => throw new NotSupportedException($"意思決定通知として未対応の状態です。実際:{state.Name}"),
        };
    }

    private static CallNotification BuildCallNotification(PlayerRoundView view, Round round, ImmutableArray<PlayerIndex> inquired)
    {
        var callerIndex = round.Turn;
        var madeCall = round.CallListArray[callerIndex].Last();
        return new CallNotification(view, madeCall, callerIndex, inquired);
    }

    private static DahaiNotification BuildDahaiNotification(PlayerRoundView view, Round round, CandidateList candidates, ImmutableArray<PlayerIndex> inquired)
    {
        var discarderIndex = round.Turn;
        var discardedTile = round.RiverArray[discarderIndex].Last();
        return new DahaiNotification(view, discardedTile, discarderIndex, candidates, inquired);
    }

    private static KanNotification BuildKanNotification(PlayerRoundView view, Round round, RoundStateKan kan, CandidateList candidates, ImmutableArray<PlayerIndex> inquired)
    {
        var kanCall = round.CallListArray[round.Turn].Last(x => x.Type == kan.KanType);
        return new KanNotification(view, kanCall, round.Turn, candidates, inquired);
    }
}
