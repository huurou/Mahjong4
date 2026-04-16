using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 打牌候補の個別選択肢 牌ごとの立直可否を保持する
/// </summary>
/// <param name="Tile">打牌可能な牌</param>
/// <param name="RiichiAvailable">この牌を打牌する場合に立直宣言可能か</param>
public record DahaiOption(Tile Tile, bool RiichiAvailable);
