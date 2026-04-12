namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 局状態遷移イベントの基底クラス
/// </summary>
public abstract record RoundEvent
{
    /// <summary>
    /// イベント名
    /// </summary>
    public abstract string Name { get; }
}
