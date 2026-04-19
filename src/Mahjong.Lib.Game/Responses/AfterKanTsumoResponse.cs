using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 嶺上ツモ後の応答 (手番プレイヤー用)
/// Design.md 準拠で RoundStateKanTsumo + RoundStateAfterKanTsumo を1通知にまとめる
/// RoundStateContext の通知・応答集約ループが応答を内部状態遷移に分解する
/// </summary>
[JsonDerivedType(typeof(RinshanTsumoResponse), nameof(RinshanTsumoResponse))]
[JsonDerivedType(typeof(KanTsumoDahaiResponse), nameof(KanTsumoDahaiResponse))]
[JsonDerivedType(typeof(KanTsumoAnkanResponse), nameof(KanTsumoAnkanResponse))]
[JsonDerivedType(typeof(KanTsumoKakanResponse), nameof(KanTsumoKakanResponse))]
public abstract record AfterKanTsumoResponse : PlayerResponse;
