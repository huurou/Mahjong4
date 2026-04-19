namespace Mahjong.Lib.Game.Games.Scoring;

/// <summary>
/// 役情報 点数計算結果の明細として使用する
/// Lib.Game は Lib.Scoring に依存しないため、Lib.Scoring の Yaku と同じ番号体系で抽象的に保持する
/// </summary>
/// <param name="Number">役番号 (天鳳準拠、Lib.Scoring の Yaku.Number と同じ体系)</param>
/// <param name="Name">役名 ("平和", "断么九" 等)</param>
/// <param name="Han">翻数 (役満は null)</param>
/// <param name="YakumanCount">役満倍数 (0 = 通常役、1 = 役満、2 = ダブル役満)</param>
/// <param name="IsPaoEligible">包 (責任払い) 対象役満かどうか (大三元 / 大四喜 / 四槓子 の場合 true)</param>
public record YakuInfo(
    int Number,
    string Name,
    int? Han,
    int YakumanCount = 0,
    bool IsPaoEligible = false
);
