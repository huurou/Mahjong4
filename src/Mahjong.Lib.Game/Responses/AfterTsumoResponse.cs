namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// ツモ後の応答 (手番プレイヤー用)
/// 打牌 / 暗槓 / 加槓 / ツモ和了 / 九種九牌 のいずれか
/// </summary>
public abstract record AfterTsumoResponse : PlayerResponse;
