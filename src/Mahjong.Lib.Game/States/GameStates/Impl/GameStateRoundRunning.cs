using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 局進行中
/// 遷移時アクションで <see cref="GameStateContext.StartRound"/> が実行される。
/// 局終了時は <see cref="RoundEndNotification"/> 送信と次局 <see cref="RoundStartNotification"/> 送信 (続行時) または
/// <see cref="GameEndNotification"/> 送信 (終了時) を遷移時アクションに集約する
/// </summary>
public record GameStateRoundRunning : GameState
{
    public override string Name => "局進行中";

    public override Task RoundEndedByWinAsync(GameStateContext context, GameEventRoundEndedByWin evt, CancellationToken ct = default)
    {
        var dealerIndex = context.Game.RoundNumber.ToDealer();
        var dealerContinues = context.Game.Rules.RenchanCondition switch
        {
            RenchanCondition.None => false,
            RenchanCondition.AgariOnly => evt.WinnerIndices.Contains(dealerIndex),
            RenchanCondition.AgariOrTenpai => evt.WinnerIndices.Contains(dealerIndex),
            _ => throw new InvalidOperationException($"未対応の連荘条件: {context.Game.Rules.RenchanCondition}"),
        };
        var mode = dealerContinues
            ? RoundAdvanceMode.Renchan
            : RoundAdvanceMode.DealerChangeResetHonba;
        return RoundEndInnerAsync(context, evt, dealerContinues, mode, ct);
    }

    /// <summary>
    /// 流局による局終了時の連荘判定 (天鳳ルール準拠):
    /// - 途中流局: 連荘条件に関わらず常に親続行 (本場+1、点数移動なし)
    /// - 荒牌平局:
    ///   - 親流し満貫: 連荘条件と独立に親続行 (天鳳ルール固定)
    ///   - 連荘条件 None: 親流れ
    ///   - 連荘条件 AgariOnly: 親流れ (流局時は親テンパイでも親流れ)
    ///   - 連荘条件 AgariOrTenpai (天鳳デフォルト): 親テンパイで連荘
    /// </summary>
    public override Task RoundEndedByRyuukyokuAsync(GameStateContext context, GameEventRoundEndedByRyuukyoku evt, CancellationToken ct = default)
    {
        var dealerContinues = ComputeDealerContinuesForRyuukyoku(context, evt);
        var mode = dealerContinues
            ? RoundAdvanceMode.Renchan
            : RoundAdvanceMode.DealerChangeWithHonba;
        return RoundEndInnerAsync(context, evt, dealerContinues, mode, ct);
    }

    private static bool ComputeDealerContinuesForRyuukyoku(GameStateContext context, GameEventRoundEndedByRyuukyoku evt)
    {
        var dealerIndex = context.Game.RoundNumber.ToDealer();
        return evt.Type switch
        {
            RyuukyokuType.KyuushuKyuuhai or
            RyuukyokuType.Suufonrenda or
            RyuukyokuType.Suukaikan or
            RyuukyokuType.SuuchaRiichi or
            RyuukyokuType.SanchaHou => true,
            RyuukyokuType.KouhaiHeikyoku => evt.NagashiManganPlayers.Contains(dealerIndex) || context.Game.Rules.RenchanCondition switch
            {
                RenchanCondition.None => false,
                RenchanCondition.AgariOnly => false,
                RenchanCondition.AgariOrTenpai => evt.TenpaiPlayers.Contains(dealerIndex),
                _ => throw new InvalidOperationException($"未対応の連荘条件: {context.Game.Rules.RenchanCondition}"),
            },
            _ => throw new InvalidOperationException($"未対応の流局種別: {evt.Type}"),
        };
    }

    private static async Task RoundEndInnerAsync(
        GameStateContext context,
        GameEvent roundEndEvent,
        bool dealerContinues,
        RoundAdvanceMode advanceMode,
        CancellationToken ct
    )
    {
        var endedRound = context.RoundStateContext?.Round
            ?? throw new InvalidOperationException("RoundStateContext がありません。");

        context.Game = context.Game.ApplyRoundResult(endedRound);
        var resolvedAction = BuildAdoptedRoundAction(roundEndEvent, dealerContinues);

        if (GameEndPolicy.ShouldEndAfterRound(context.Game, roundEndEvent, dealerContinues))
        {
            await TransitAsync(
                context,
                new GameStateEnd(),
                action: async () =>
                {
                    if (context.IsRoundManagerAvailable)
                    {
                        await context.BroadcastGameNotificationAsync(
                            _ => new RoundEndNotification(resolvedAction), ct);
                    }
                    context.DisposeRoundContext();
                    if (context.IsRoundManagerAvailable)
                    {
                        await context.BroadcastGameNotificationAsync(
                            _ => new GameEndNotification(context.Game.PointArray), ct);
                    }
                },
                ct
            );
        }
        else
        {
            context.Game = context.Game.AdvanceToNextRound(advanceMode);
            await TransitAsync(
                context,
                new GameStateRoundRunning(),
                action: async () =>
                {
                    if (context.IsRoundManagerAvailable)
                    {
                        await context.BroadcastGameNotificationAsync(
                            _ => new RoundEndNotification(resolvedAction), ct);
                    }
                    context.DisposeRoundContext();
                    if (context.IsRoundManagerAvailable)
                    {
                        await context.BroadcastGameNotificationAsync(
                            _ => new RoundStartNotification(
                                context.Game.RoundWind,
                                context.Game.RoundNumber,
                                context.Game.Honba,
                                context.Game.RoundNumber.ToDealer()
                            ),
                            ct
                        );
                    }
                    context.StartRound(context.Game.CreateRound(context.WallGenerator));
                },
                ct
            );
        }
    }

    private static AdoptedRoundAction BuildAdoptedRoundAction(GameEvent evt, bool dealerContinues)
    {
        return evt switch
        {
            GameEventRoundEndedByWin win => new AdoptedWinAction(
                winnerIndices: [
                    .. win.Winners.IsDefaultOrEmpty
                        ? win.WinnerIndices.Select(x => new AdoptedWinner(x, default!, default!))
                        : win.Winners
                ],
                loserIndex: win.WinType is WinType.Tsumo or WinType.Rinshan ? null : win.LoserIndex,
                winType: win.WinType,
                kyoutakuRiichiAward: win.KyoutakuRiichiAward,
                honba: win.Honba ?? new Honba(0),
                dealerContinues: dealerContinues
            ),
            GameEventRoundEndedByRyuukyoku ryu => new AdoptedRyuukyokuAction(
                Type: ryu.Type,
                TenpaiPlayerIndices: [.. ryu.TenpaiPlayers],
                NagashiManganPlayerIndices: [.. ryu.NagashiManganPlayers],
                DealerContinues: dealerContinues
            ),
            _ => throw new NotSupportedException($"未対応のイベント: {evt?.GetType().Name}"),
        };
    }
}
