namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 和了
/// 全プレイヤーに和了による局終了を通知し、OK応答で <see cref="RoundStateContext.RoundEnded"/> を発火する
/// 精算は本状態への遷移時アクションで完了しており、本状態自身はロジックを持たない
/// </summary>
/// <param name="EventArgs">局終了通知に渡す情報 (和了者・放銃者・和了種別)</param>
public record RoundStateWin(RoundEndedByWinEventArgs EventArgs) : RoundState
{
    public override string Name => "和了";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        context.OnRoundEnded(EventArgs);
    }
}
