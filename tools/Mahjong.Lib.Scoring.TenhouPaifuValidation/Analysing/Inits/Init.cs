using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Inits;

/// <summary>
/// INITノードオブジェクト
/// </summary>
/// <param name="Kyoku">局順 0はじまりで東一局からカウント</param>
/// <param name="Honba">何本場か</param>
/// <param name="RoundWind">場風</param>
/// <param name="Oya">親番の対局者の番号</param>
public record Init(int Kyoku, int Honba, Wind RoundWind, int Oya);
