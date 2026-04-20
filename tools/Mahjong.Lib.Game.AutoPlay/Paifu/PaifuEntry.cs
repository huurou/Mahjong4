using System.Collections.Immutable;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// 牌譜エントリ (JSONL の 1 行)
/// ドメイン型と疎結合に、基本情報のみを持たせる
/// </summary>
/// <param name="Type">イベント種別 (round-start / notify / response / round-end 等)</param>
/// <param name="Fields">付随情報</param>
public record PaifuEntry(string Type, ImmutableDictionary<string, object?> Fields);
