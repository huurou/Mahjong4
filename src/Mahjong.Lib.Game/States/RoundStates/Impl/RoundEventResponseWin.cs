namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 和了応答イベント
/// </summary>
public record RoundEventResponseWin : RoundEvent
{
    public override string Name => "和了応答";
}
