using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 優先順位適用後の採用済み局アクション
/// RoundManager が応答を集約・優先順位解決した結果
/// </summary>
[JsonDerivedType(typeof(ResolvedOkAction), nameof(ResolvedOkAction))]
[JsonDerivedType(typeof(ResolvedDahaiAction), nameof(ResolvedDahaiAction))]
[JsonDerivedType(typeof(ResolvedCallAction), nameof(ResolvedCallAction))]
[JsonDerivedType(typeof(ResolvedAnkanAction), nameof(ResolvedAnkanAction))]
[JsonDerivedType(typeof(ResolvedKakanAction), nameof(ResolvedKakanAction))]
[JsonDerivedType(typeof(ResolvedWinAction), nameof(ResolvedWinAction))]
[JsonDerivedType(typeof(ResolvedRyuukyokuAction), nameof(ResolvedRyuukyokuAction))]
public abstract record ResolvedRoundAction;
