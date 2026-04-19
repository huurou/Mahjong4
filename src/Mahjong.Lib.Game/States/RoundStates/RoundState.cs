using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
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
    /// 打牌応答
    /// </summary>
    public virtual void ResponseDahai(RoundStateContext context, RoundEventResponseDahai evt) { }

    /// <summary>
    /// 副露応答
    /// </summary>
    public virtual void ResponseCall(RoundStateContext context, RoundEventResponseCall evt) { }

    /// <summary>
    /// 槓応答
    /// </summary>
    public virtual void ResponseKan(RoundStateContext context, RoundEventResponseKan evt) { }

    /// <summary>
    /// 和了応答
    /// </summary>
    public virtual void ResponseWin(RoundStateContext context, RoundEventResponseWin evt) { }

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
    /// この状態でプレイヤーへ送る問い合わせ仕様 (誰に・何を聞くか) を返す。
    /// Call/Win/Ryuukyoku のような通知観測点は全員に OkCandidate のみを提示する。
    /// </summary>
    public abstract RoundInquirySpec CreateInquirySpec(Round round, IResponseCandidateEnumerator enumerator);

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

    /// <summary>
    /// 遷移時アクション実行後に遷移先状態を生成して遷移します
    /// </summary>
    /// <param name="context">状態遷移コンテキスト</param>
    /// <param name="nextStateFactory">action 実行後に評価される遷移先状態ファクトリ</param>
    /// <param name="action">遷移時アクション</param>
    protected static void Transit(RoundStateContext context, Func<RoundState> nextStateFactory, Action? action = null)
    {
        context.Transit(nextStateFactory, action);
    }
}
