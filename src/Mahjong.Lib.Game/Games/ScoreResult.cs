using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 点数計算結果
/// 本場・供託の加算は本結果には含めず、呼び出し側 (RoundStateWin.Entry) で別途適用する
/// </summary>
/// <param name="Han">翻数</param>
/// <param name="Fu">符数</param>
/// <param name="PointDeltas">プレイヤー別の純粋な点数移動 (和了者は正、放銃者・他家は負)</param>
/// <param name="YakuList">成立した役のリスト (Lib.Scoring の <see cref="Yaku"/> 集合)</param>
/// <param name="IsMenzen">門前 (副露なし、または暗槓のみ) 和了かどうか
/// 表示時に <see cref="Yaku.HanClosed"/> / <see cref="Yaku.HanOpen"/> のどちらを参照するかの判定材料として保持する</param>
public record ScoreResult(
    int Han,
    int Fu,
    PointArray PointDeltas,
    YakuList YakuList,
    bool IsMenzen
);
