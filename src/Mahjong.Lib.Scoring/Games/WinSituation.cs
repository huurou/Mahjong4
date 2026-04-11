namespace Mahjong.Lib.Scoring.Games;

/// <summary>
/// 和了時の状況
/// </summary>
public record WinSituation
{
    /// <summary>
    /// ツモアガリかどうか
    /// </summary>
    public bool IsTsumo { get; init; } = false;

    /// <summary>
    /// 立直しているかどうか
    /// </summary>
    public bool IsRiichi { get; init; } = false;

    /// <summary>
    /// 一発かどうか
    /// </summary>
    public bool IsIppatsu { get; init; } = false;

    /// <summary>
    /// 槍槓かどうか
    /// </summary>
    public bool IsChankan { get; init; } = false;

    /// <summary>
    /// 嶺上開花かどうか
    /// </summary>
    public bool IsRinshan { get; init; } = false;

    /// <summary>
    /// 海底撈月かどうか
    /// </summary>
    public bool IsHaitei { get; init; } = false;

    /// <summary>
    /// 河底撈魚かどうか
    /// </summary>
    public bool IsHoutei { get; init; } = false;

    /// <summary>
    /// ダブル立直かどうか
    /// </summary>
    public bool IsDoubleRiichi { get; init; } = false;

    /// <summary>
    /// 流し満貫かどうか
    /// </summary>
    public bool IsNagashimangan { get; init; } = false;

    /// <summary>
    /// 天和かどうか
    /// </summary>
    public bool IsTenhou { get; init; } = false;

    /// <summary>
    /// 地和かどうか
    /// </summary>
    public bool IsChiihou { get; init; } = false;

    /// <summary>
    /// 人和かどうか
    /// </summary>
    public bool IsRenhou { get; init; } = false;

    /// <summary>
    /// 和了者の風
    /// </summary>
    public Wind PlayerWind { get; init; } = Wind.East;

    /// <summary>
    /// 場風
    /// </summary>
    public Wind RoundWind { get; init; } = Wind.East;

    /// <summary>
    /// プレイヤーが親かどうか
    /// </summary>
    public bool IsDealer => PlayerWind == Wind.East;

    /// <summary>
    /// 赤ドラの枚数 どの牌が赤ドラで何枚あるかはゲーム側で管理してもらう
    /// </summary>
    public int AkadoraCount { get; init; } = 0;
}
