using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 他家の槓 (加槓) 後のアクション応答
/// 槍槓ロンのみ
/// スルーはこの階層ではなく <see cref="OkResponse"/> を返す
/// </summary>
[JsonDerivedType(typeof(ChankanRonResponse), nameof(ChankanRonResponse))]
public abstract record AfterKanResponse : PlayerResponse;
