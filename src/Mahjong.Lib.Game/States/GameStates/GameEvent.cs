namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局イベントの基底クラス
/// </summary>
public abstract record GameEvent
{
    /// <summary>
    /// イベント名
    /// </summary>
    public abstract string Name { get; }
}
