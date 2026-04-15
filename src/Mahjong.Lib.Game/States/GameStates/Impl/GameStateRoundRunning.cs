using Mahjong.Lib.Game.Games;

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
        var dealer = endedRound.RoundNumber.ToDealer();
        var dealerContinues = context.Game.Rules.RenchanCondition switch
        {
            RenchanCondition.None => false,
            RenchanCondition.AgariOnly => evt.Winners.Contains(dealer),
            // TODO: Phase 2 で親テンパイ時の連荘を追加 (現状 AgariOnly 相当)
            RenchanCondition.AgariOrTenpai => evt.Winners.Contains(dealer),
            _ => throw new InvalidOperationException($"未対応の連荘条件: {context.Game.Rules.RenchanCondition}"),
        };
        var mode = dealerContinues
            ? RoundAdvanceMode.Renchan
            : RoundAdvanceMode.DealerChangeResetHonba;
        HandleRoundEnd(context, evt, dealerContinues, mode);
    }

    public override void RoundEndedByRyuukyoku(GameStateContext context, GameEventRoundEndedByRyuukyoku evt)
    {
        // Phase 1 スタブ 親テンパイ判定は Phase 2 で正確化 常に親流れ+本場+1
        HandleRoundEnd(context, evt, dealerContinues: false, RoundAdvanceMode.DealerChangeWithHonba);
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
