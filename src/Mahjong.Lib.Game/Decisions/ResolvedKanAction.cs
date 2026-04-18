using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 槓 (暗槓/加槓)採用結果
/// </summary>
[JsonDerivedType(typeof(ResolvedAnkanAction), nameof(ResolvedAnkanAction))]
[JsonDerivedType(typeof(ResolvedKakanAction), nameof(ResolvedKakanAction))]
public abstract record ResolvedKanAction : ResolvedRoundAction;
