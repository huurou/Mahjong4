namespace Mahjong.Lib.ScoreCalcValidation.Downloads;

/// <summary>
/// 牌譜を表現するクラス
/// </summary>
/// <param name="GameId">ゲームId</param>
/// <param name="Content">牌譜の内容</param>
public record Paifu(string GameId, string Content);
