namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 対局終了 (終端状態)
/// </summary>
public record GameStateEnd : GameState
{
    public override string Name => "対局終了";
}
