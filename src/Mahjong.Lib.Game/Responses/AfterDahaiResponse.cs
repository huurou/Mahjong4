using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 他家打牌後のアクション応答 (受け手プレイヤー用)
/// チー / ポン / 大明槓 / ロン のいずれか
/// スルーはこの階層ではなく <see cref="OkResponse"/> を返す
/// </summary>
[JsonDerivedType(typeof(ChiResponse), nameof(ChiResponse))]
[JsonDerivedType(typeof(PonResponse), nameof(PonResponse))]
[JsonDerivedType(typeof(DaiminkanResponse), nameof(DaiminkanResponse))]
[JsonDerivedType(typeof(RonResponse), nameof(RonResponse))]
public abstract record AfterDahaiResponse : PlayerResponse;
