using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Adoptions;

/// <summary>
/// 槓 (暗槓/加槓)採用結果
/// </summary>
[JsonDerivedType(typeof(AdoptedAnkanAction), nameof(AdoptedAnkanAction))]
[JsonDerivedType(typeof(AdoptedKakanAction), nameof(AdoptedKakanAction))]
public abstract record AdoptedKanAction : AdoptedRoundAction;
