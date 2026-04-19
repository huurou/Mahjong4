using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 応答候補の基底型 プレイヤーが選択可能な合法アクションを表す
/// </summary>
[JsonDerivedType(typeof(OkCandidate), nameof(OkCandidate))]
[JsonDerivedType(typeof(DahaiCandidate), nameof(DahaiCandidate))]
[JsonDerivedType(typeof(ChiCandidate), nameof(ChiCandidate))]
[JsonDerivedType(typeof(PonCandidate), nameof(PonCandidate))]
[JsonDerivedType(typeof(DaiminkanCandidate), nameof(DaiminkanCandidate))]
[JsonDerivedType(typeof(AnkanCandidate), nameof(AnkanCandidate))]
[JsonDerivedType(typeof(KakanCandidate), nameof(KakanCandidate))]
[JsonDerivedType(typeof(RonCandidate), nameof(RonCandidate))]
[JsonDerivedType(typeof(TsumoAgariCandidate), nameof(TsumoAgariCandidate))]
[JsonDerivedType(typeof(ChankanRonCandidate), nameof(ChankanRonCandidate))]
[JsonDerivedType(typeof(RinshanTsumoAgariCandidate), nameof(RinshanTsumoAgariCandidate))]
[JsonDerivedType(typeof(KyuushuKyuuhaiCandidate), nameof(KyuushuKyuuhaiCandidate))]
public abstract record ResponseCandidate;
