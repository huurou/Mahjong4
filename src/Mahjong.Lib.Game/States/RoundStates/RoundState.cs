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
    /// 遷移先状態を生成して遷移します。
    /// 順序は Exit → updateRound 適用 (null 以外のとき) → createNextState 評価 → State 差替 → Entry。
    /// イミュータブル集約 <see cref="Round"/> の外部書き換えを防ぐため、状態遷移時の Round 更新はこの経路のみで行う。
    /// updateRound 適用後の <see cref="RoundStateContext.Round"/> を遷移先のプロパティへ封入したい場合は
    /// createNextState 内で <c>context.Round</c> を参照する (例: <see cref="Impl.RoundStateCall.SnapshotRound"/>)
    /// </summary>
    /// <param name="context">状態遷移コンテキスト</param>
    /// <param name="createNextState">updateRound 適用後に評価される遷移先状態ファクトリ</param>
    /// <param name="updateRound">遷移時 Round 更新関数 (現 Round を受け取り新 Round を返す)。null の場合は Round を更新しない</param>
    protected static void Transit(RoundStateContext context, Func<RoundState> createNextState, Func<Round, Round>? updateRound = null)
    {
        context.Transit(createNextState, updateRound);
    }
}
