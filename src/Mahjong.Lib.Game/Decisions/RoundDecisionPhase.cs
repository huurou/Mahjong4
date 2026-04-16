namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 局の意思決定フェーズ RoundDecisionSpec で使用する
/// </summary>
public enum RoundDecisionPhase
{
    /// <summary>
    /// 配牌 全プレイヤーにOK応答を求める
    /// </summary>
    Haipai,

    /// <summary>
    /// ツモ 手番プレイヤーに打牌/暗槓/加槓/ツモ和了/九種九牌の選択を求める
    /// </summary>
    Tsumo,

    /// <summary>
    /// 打牌 他家にチー/ポン/大明槓/ロン/OKの選択を求める
    /// </summary>
    Dahai,

    /// <summary>
    /// 槓 (加槓) 他家に槍槓ロン/OKの選択を求める
    /// </summary>
    Kan,

    /// <summary>
    /// 嶺上ツモ 手番プレイヤーに嶺上ツモ和了/打牌/暗槓/加槓の選択を求める
    /// </summary>
    KanTsumo,

    /// <summary>
    /// 嶺上ツモ後 嶺上ツモ和了しなかった場合の打牌/暗槓/加槓の選択
    /// </summary>
    AfterKanTsumo,
}
