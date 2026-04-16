using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 加槓採用結果
/// </summary>
/// <param name="Tile">追加する牌</param>
public record ResolvedKakanAction(Tile Tile) : ResolvedKanAction;
