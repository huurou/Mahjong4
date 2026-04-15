namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 流局による局終了イベント
/// </summary>
/// <param name="Type">流局種別</param>
public record GameEventRoundEndedByRyuukyoku(RyuukyokuType Type) : GameEvent
{
    public override string Name => "局終了(流局)";
}
