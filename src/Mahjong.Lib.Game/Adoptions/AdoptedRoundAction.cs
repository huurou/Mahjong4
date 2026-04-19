using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Adoptions;

/// <summary>
/// 優先順位適用後の採用済み局アクション
/// RoundStateContext の通知・応答集約ループが応答を集約・優先順位解決した結果
/// </summary>
[JsonDerivedType(typeof(AdoptedOkAction), nameof(AdoptedOkAction))]
[JsonDerivedType(typeof(AdoptedDahaiAction), nameof(AdoptedDahaiAction))]
[JsonDerivedType(typeof(AdoptedCallAction), nameof(AdoptedCallAction))]
[JsonDerivedType(typeof(AdoptedAnkanAction), nameof(AdoptedAnkanAction))]
[JsonDerivedType(typeof(AdoptedKakanAction), nameof(AdoptedKakanAction))]
[JsonDerivedType(typeof(AdoptedWinAction), nameof(AdoptedWinAction))]
[JsonDerivedType(typeof(AdoptedRyuukyokuAction), nameof(AdoptedRyuukyokuAction))]
public abstract record AdoptedRoundAction;
