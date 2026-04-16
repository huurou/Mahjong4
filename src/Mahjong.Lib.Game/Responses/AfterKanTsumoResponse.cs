namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 嶺上ツモ後の応答 (手番プレイヤー用)
/// Design.md 準拠で RoundStateKanTsumo + RoundStateAfterKanTsumo を1通知にまとめる
/// RoundManager が応答を内部状態遷移に分解する
/// </summary>
public abstract record AfterKanTsumoResponse : PlayerResponse;
