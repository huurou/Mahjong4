using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Games.Scoring;

/// <summary>
/// 点数計算結果
/// 本場・供託の加算は本結果には含めず、呼び出し側 (RoundStateWin.Entry) で別途適用する
/// </summary>
/// <param name="Han">翻数</param>
/// <param name="Fu">符数</param>
/// <param name="PointDeltas">プレイヤー別の純粋な点数移動 (和了者は正、放銃者・他家は負)</param>
/// <param name="YakuInfos">成立した役の一覧 (表示・ログ用)</param>
public record ScoreResult(
    int Han,
    int Fu,
    PointArray PointDeltas,
    ImmutableList<YakuInfo> YakuInfos
);
