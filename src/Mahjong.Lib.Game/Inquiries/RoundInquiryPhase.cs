namespace Mahjong.Lib.Game.Inquiries;

/// <summary>
/// 局内プレイヤー問い合わせフェーズ RoundInquirySpec で使用する
/// </summary>
public enum RoundInquiryPhase
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

    /// <summary>
    /// 副露 全プレイヤーに副露通知のOK応答を求める (副露直後の通知観測点)
    /// </summary>
    Call,

    /// <summary>
    /// 副露後 チー/ポン直後に副露者へ打牌を求める (暗槓・加槓・ツモ和了・九種九牌は不可)
    /// </summary>
    AfterCall,

    /// <summary>
    /// 和了 全プレイヤーに和了通知のOK応答を求める (終端状態)
    /// </summary>
    Win,

    /// <summary>
    /// 流局 全プレイヤーに流局通知のOK応答を求める (終端状態)
    /// </summary>
    Ryuukyoku,
}
