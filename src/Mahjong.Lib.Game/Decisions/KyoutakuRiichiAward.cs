using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 供託立直棒の受取情報
/// </summary>
/// <param name="RecipientIndex">供託を受け取るプレイヤー (上家取り)</param>
/// <param name="Count">受け取る供託棒の本数</param>
public record KyoutakuRiichiAward(PlayerIndex RecipientIndex, int Count);
