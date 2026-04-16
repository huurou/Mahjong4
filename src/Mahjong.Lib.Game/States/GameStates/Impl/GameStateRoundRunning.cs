using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 局進行中 遷移時アクションで RoundStateContext を起動する
/// </summary>
public record GameStateRoundRunning : GameState
{
    public override string Name => "局進行中";

    public override void RoundEndedByWin(GameStateContext context, GameEventRoundEndedByWin evt)
    {
        var endedRound = context.RoundStateContext?.Round
            ?? throw new InvalidOperationException("RoundStateContext がありません。");
        var dealerIndex = endedRound.RoundNumber.ToDealer();
        var dealerContinues = context.Game.Rules.RenchanCondition switch
        {
            RenchanCondition.None => false,
            // 親和了で連荘 (AgariOrTenpai も和了時の挙動は AgariOnly と同等)
            RenchanCondition.AgariOnly => evt.WinnerIndices.Contains(dealerIndex),
            RenchanCondition.AgariOrTenpai => evt.WinnerIndices.Contains(dealerIndex),
            _ => throw new InvalidOperationException($"未対応の連荘条件: {context.Game.Rules.RenchanCondition}"),
        };
        var mode = dealerContinues
            ? RoundAdvanceMode.Renchan
            : RoundAdvanceMode.DealerChangeResetHonba;
        HandleRoundEnd(context, evt, dealerContinues, mode);
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
    public override void RoundEndedByRyuukyoku(GameStateContext context, GameEventRoundEndedByRyuukyoku evt)
    {
        var dealerIndex = context.Game.RoundNumber.ToDealer();
        var dealerContinues = evt.Type switch
        {
            // 途中流局
            RyuukyokuType.KyuushuKyuuhai or
            RyuukyokuType.Suufonrenda or
            RyuukyokuType.Suukaikan or
            RyuukyokuType.SuuchaRiichi or
            RyuukyokuType.SanchaHou => true,
            // 荒牌平局
            RyuukyokuType.KouhaiHeikyoku => evt.NagashiManganPlayers.Contains(dealerIndex) || context.Game.Rules.RenchanCondition switch
            {
                RenchanCondition.None => false,
                RenchanCondition.AgariOnly => false,
                RenchanCondition.AgariOrTenpai => evt.TenpaiPlayers.Contains(dealerIndex),
                _ => throw new InvalidOperationException($"未対応の連荘条件: {context.Game.Rules.RenchanCondition}"),
            },
            _ => throw new InvalidOperationException($"未対応の流局種別: {evt.Type}"),
        };
        var mode = dealerContinues
            ? RoundAdvanceMode.Renchan
            : RoundAdvanceMode.DealerChangeWithHonba;
        HandleRoundEnd(context, evt, dealerContinues, mode);
    }

    private static void HandleRoundEnd(
        GameStateContext context,
        GameEvent roundEndEvent,
        bool dealerContinues,
        RoundAdvanceMode advanceMode
    )
    {
        var endedRound = context.RoundStateContext?.Round
            ?? throw new InvalidOperationException("RoundStateContext がありません。");

        context.Game = context.Game.ApplyRoundResult(endedRound);
        if (GameEndPolicy.ShouldEndAfterRound(context.Game, roundEndEvent, dealerContinues))
        {
            Transit(context, new GameStateEnd(), () => context.DisposeRoundContext());
        }
        else
        {
            context.Game = context.Game.AdvanceToNextRound(advanceMode);
            Transit(context, new GameStateRoundRunning(), () =>
            {
                context.DisposeRoundContext();
                context.StartRound(context.Game.CreateRound(context.WallGenerator));
            });
        }
    }
}
