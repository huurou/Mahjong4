using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;

/// <summary>
/// AGARIノードオブジェクト
/// </summary>
/// <param name="Hand">手牌 副露牌を含まない 和了牌を含む</param>
/// <param name="CallList">副露牌</param>
/// <param name="WinTile">和了牌</param>
/// <param name="Fu">符</param>
/// <param name="Score">点数の合計</param>
/// <param name="ManganType">満貫種類</param>
/// <param name="YakuInfos">役情報のリスト</param>
/// <param name="Yakumans">役満のリスト</param>
/// <param name="DoraIndicators">ドラ表示牌のリスト</param>
/// <param name="UradoraIndicators">裏ドラ表示牌のリスト</param>
/// <param name="IsTsumo">ツモかどうか</param>
/// <param name="AkadoraCount">手に含まれる赤ドラの数</param>
/// <param name="Who">和了者の番号</param>
public record Agari(
    TileKindList Hand,
    CallList CallList,
    TileKind WinTile,
    int Fu,
    int Score,
    ManganType ManganType,
    List<YakuInfo> YakuInfos,
    List<int> Yakumans,
    TileKindList DoraIndicators,
    TileKindList UradoraIndicators,
    bool IsTsumo,
    int AkadoraCount,
    int Who
);
