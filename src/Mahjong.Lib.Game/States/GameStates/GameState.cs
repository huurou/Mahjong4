using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局状態の基底クラス
/// </summary>
public abstract record GameState
{
    /// <summary>
    /// 状態名
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// OK応答
    /// </summary>
    public virtual void ResponseOk(GameStateContext context, GameEventResponseOk evt) { }

    /// <summary>
    /// 和了による局終了通知
    /// </summary>
    public virtual void RoundEndedByWin(GameStateContext context, GameEventRoundEndedByWin evt) { }

    /// <summary>
    /// 流局による局終了通知
    /// </summary>
    public virtual void RoundEndedByRyuukyoku(GameStateContext context, GameEventRoundEndedByRyuukyoku evt) { }

    /// <summary>
    /// 状態入場時の処理
    /// </summary>
    public virtual void Entry(GameStateContext context)
    {
        context.OnStateChanged(this);
    }

    /// <summary>
    /// 状態退場時の処理
    /// </summary>
    public virtual void Exit(GameStateContext context)
    {
    }

    /// <summary>
    /// 指定された状態に遷移します
    /// </summary>
    /// <param name="context">状態遷移コンテキスト</param>
    /// <param name="nextState">遷移先状態</param>
    /// <param name="action">遷移時アクション</param>
    protected static void Transit(GameStateContext context, GameState nextState, Action? action = null)
    {
        context.Transit(nextState, action);
    }
}
