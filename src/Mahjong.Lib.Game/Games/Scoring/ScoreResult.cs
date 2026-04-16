using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Games.Scoring;

/// <summary>
/// 点数計算結果
/// 本場・供託の加算は本結果には含めず、呼び出し側 (RoundStateWin.Entry) で別途適用する
/// </summary>
/// <param name="Han">翻数</param>
/// <param name="Fu">符数</param>
/// <param name="PointDeltas">プレイヤー別の純粋な点数移動 (和了者は正、放銃者・他家は負)</param>
public record ScoreResult(
    int Han,
    int Fu,
    PointArray PointDeltas
);
