using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// ツモ後の応答 (手番プレイヤー用)
/// 打牌 / 暗槓 / 加槓 / ツモ和了 / 九種九牌 のいずれか
/// </summary>
[JsonDerivedType(typeof(DahaiResponse), nameof(DahaiResponse))]
[JsonDerivedType(typeof(TsumoAgariResponse), nameof(TsumoAgariResponse))]
[JsonDerivedType(typeof(AnkanResponse), nameof(AnkanResponse))]
[JsonDerivedType(typeof(KakanResponse), nameof(KakanResponse))]
[JsonDerivedType(typeof(KyuushuKyuuhaiResponse), nameof(KyuushuKyuuhaiResponse))]
public abstract record AfterTsumoResponse : PlayerResponse;
