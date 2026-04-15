using Mahjong.Lib.Game.States.RoundStates.Impl;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 局状態の基底クラス
/// </summary>
public abstract record RoundState
{
    /// <summary>
    /// 状態名
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// OK応答
    /// </summary>
    public virtual void ResponseOk(RoundStateContext context, RoundEventResponseOk evt) { }

    /// <summary>
    /// 和了応答
    /// </summary>
    public virtual void ResponseWin(RoundStateContext context, RoundEventResponseWin evt) { }

    /// <summary>
    /// 打牌応答
    /// </summary>
    public virtual void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt) { }

    /// <summary>
    /// 槓応答
    /// </summary>
    public virtual void ResponseKan(RoundStateContext context, RoundEventResponseKan evt) { }

    /// <summary>
    /// 副露応答
    /// </summary>
    public virtual void ResponseCall(RoundStateContext context, RoundEventResponseCall evt) { }

    /// <summary>
    /// 流局応答
    /// </summary>
    public virtual void ResponseRyuukyoku(RoundStateContext context, RoundEventResponseRyuukyoku evt) { }

    /// <summary>
    /// 状態入場時の処理
    /// </summary>
    public virtual void Entry(RoundStateContext context)
    {
        context.OnStateChanged(this);
    }

    /// <summary>
    /// 状態退場時の処理
    /// </summary>
    public virtual void Exit(RoundStateContext context)
    {
    }

    /// <summary>
    /// 指定された状態に遷移します
    /// </summary>
    /// <param name="context">状態遷移コンテキスト</param>
    /// <param name="nextState">遷移先状態</param>
    /// <param name="action">遷移時アクション</param>
    protected static void Transit(RoundStateContext context, RoundState nextState, Action? action = null)
    {
        context.Transit(nextState, action);
    }
}
