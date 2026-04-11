namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Rounds;

/// <summary>
/// 局データ
/// </summary>
/// <param name="GameId">対局Id</param>
/// <param name="InitTag">INITノード</param>
/// <param name="AgariTags">AGARIノードのリスト ダブロンの場合複数のAGARIがあるため</param>
public record RoundData(string GameId, string InitTag, List<string> AgariTags);
