namespace Mahjong.Lib.Game.Responses;

/// <summary>
/// 他家打牌後の応答 (受け手プレイヤー用)
/// スルー / チー / ポン / 大明槓 / ロン のいずれか
/// </summary>
public abstract record AfterDahaiResponse : PlayerResponse;
