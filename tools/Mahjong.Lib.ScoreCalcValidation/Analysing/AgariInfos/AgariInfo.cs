using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;

namespace Mahjong.Lib.ScoreCalcValidation.Analysing.AgariInfos;

/// <summary>
/// 牌譜から抽出した和了情報 点数計算検証の入力と期待値を同時に保持する
/// </summary>
/// <param name="GameId">対局Id</param>
/// <param name="Kyoku">局順 0はじまりで東一局からカウント</param>
/// <param name="Honba">何本場か</param>
/// <param name="TileKindList">手牌（副露を含まない）</param>
/// <param name="WinTile">和了牌</param>
/// <param name="CallList">副露リスト</param>
/// <param name="DoraIndicators">ドラ表示牌のリスト</param>
/// <param name="UradoraIndicators">裏ドラ表示牌のリスト</param>
/// <param name="WinSituation">和了状況</param>
/// <param name="Fu">符（牌譜の期待値）</param>
/// <param name="Han">翻（牌譜の期待値）</param>
/// <param name="TotalScore">点数合計（牌譜の期待値）</param>
/// <param name="YakuList">役リスト（牌譜の期待値）</param>
/// <param name="ManganType">満貫種別</param>
public record AgariInfo(
    // ヘッダー
    string GameId,
    int Kyoku,
    int Honba,
    // 引数部
    TileKindList TileKindList,
    TileKind WinTile,
    CallList CallList,
    TileKindList DoraIndicators,
    TileKindList UradoraIndicators,
    WinSituation WinSituation,
    // 結果部
    int Fu,
    int Han,
    int TotalScore,
    YakuList YakuList,
    // 補足
    ManganType ManganType
);
