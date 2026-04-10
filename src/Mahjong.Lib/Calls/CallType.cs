namespace Mahjong.Lib.Calls;

/// <summary>
/// 副露種別
/// </summary>
public enum CallType
{
    /// <summary>
    /// チー
    /// </summary>
    Chi,
    /// <summary>
    /// ポン
    /// </summary>
    Pon,
    /// <summary>
    /// 暗槓
    /// </summary>
    Ankan,
    /// <summary>
    /// 明槓
    /// </summary>
    Minkan,
    /// <summary>
    /// 抜き
    /// </summary>
    Nuki,
}

/// <summary>
/// CallTypeの拡張メソッドを提供するクラスです
/// </summary>
public static class CallTypeExtensions
{
    /// <summary>
    /// 副露種別の日本語文字列表現を返します
    /// </summary>
    /// <param name="callType">副露種別</param>
    /// <returns>副露種別の日本語文字列</returns>
    public static string ToStr(this CallType callType)
    {
        return callType switch
        {
            CallType.Chi => "チー",
            CallType.Pon => "ポン",
            CallType.Ankan => "暗槓",
            CallType.Minkan => "明槓",
            CallType.Nuki => "抜き",
            _ => throw new ArgumentOutOfRangeException(nameof(callType), callType, null),
        };
    }
}
