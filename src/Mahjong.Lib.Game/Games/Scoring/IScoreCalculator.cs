namespace Mahjong.Lib.Game.Games.Scoring;

/// <summary>
/// 和了時点数計算の抽象
/// 実装は Mahjong.Lib.Scoring に依存しない形で Lib.Game 外部 (ApiService 等) から注入する
/// </summary>
public interface IScoreCalculator
{
    /// <summary>
    /// 指定の和了状況から点数計算を行います
    /// </summary>
    ScoreResult Calculate(ScoreRequest request);
}
