namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 流局
/// 全プレイヤーに流局による局終了を通知し、OK応答で <see cref="RoundStateContext.RoundEnded"/> を発火する
/// 精算は本状態への遷移時アクションで完了しており、本状態自身はロジックを持たない
/// </summary>
/// <param name="EventArgs">局終了通知に渡す情報 (流局種別・テンパイ者・流し満貫者)</param>
public record RoundStateRyuukyoku(RoundEndedByRyuukyokuEventArgs EventArgs) : RoundState
{
    public override string Name => "流局";

    public override void ResponseOk(RoundStateContext context, RoundEventResponseOk evt)
    {
        base.ResponseOk(context, evt);
        context.OnRoundEnded(EventArgs);
    }
}
