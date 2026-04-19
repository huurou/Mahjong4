namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 対局終了 (終端状態)
/// 入場時点で対局進行は完了しており、<see cref="Notifications.GameEndNotification"/> は遷移時アクション側で送信される
/// </summary>
public record GameStateEnd : GameState
{
    public override string Name => "対局終了";
}
