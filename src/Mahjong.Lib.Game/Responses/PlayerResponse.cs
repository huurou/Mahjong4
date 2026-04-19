using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// プレイヤー応答の基底型
/// スルー (パス) 応答は専用型を設けず <see cref="OkResponse"/> を返す
/// </summary>
[JsonDerivedType(typeof(OkResponse), nameof(OkResponse))]
[JsonDerivedType(typeof(DahaiResponse), nameof(DahaiResponse))]
[JsonDerivedType(typeof(TsumoAgariResponse), nameof(TsumoAgariResponse))]
[JsonDerivedType(typeof(AnkanResponse), nameof(AnkanResponse))]
[JsonDerivedType(typeof(KakanResponse), nameof(KakanResponse))]
[JsonDerivedType(typeof(KyuushuKyuuhaiResponse), nameof(KyuushuKyuuhaiResponse))]
[JsonDerivedType(typeof(ChiResponse), nameof(ChiResponse))]
[JsonDerivedType(typeof(PonResponse), nameof(PonResponse))]
[JsonDerivedType(typeof(DaiminkanResponse), nameof(DaiminkanResponse))]
[JsonDerivedType(typeof(RonResponse), nameof(RonResponse))]
[JsonDerivedType(typeof(ChankanRonResponse), nameof(ChankanRonResponse))]
[JsonDerivedType(typeof(RinshanTsumoResponse), nameof(RinshanTsumoResponse))]
[JsonDerivedType(typeof(KanTsumoDahaiResponse), nameof(KanTsumoDahaiResponse))]
[JsonDerivedType(typeof(KanTsumoAnkanResponse), nameof(KanTsumoAnkanResponse))]
[JsonDerivedType(typeof(KanTsumoKakanResponse), nameof(KanTsumoKakanResponse))]
public abstract record PlayerResponse;
