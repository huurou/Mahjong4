using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// プレイヤー応答が提示済み候補に含まれる合法応答かを検証する
/// 候補外応答は RoundManager 側で defaultFactory によるフォールバックに差し替えられる
/// </summary>
internal static class ResponseValidator
{
    /// <summary>
    /// 指定のプレイヤー応答が <paramref name="candidates"/> に含まれる合法応答かを返します。
    /// 牌単位の一致は以下の規約で判定します:
    /// - 打牌 / 嶺上ツモ打牌: DahaiCandidate.DahaiOptionList 内の Tile と完全一致 (Tile.Id 一致)
    /// - 立直宣言 (IsRiichi=true): 対応 DahaiOption.RiichiAvailable=true が必要
    /// - 暗槓 / 嶺上暗槓: 候補の先頭牌と Tile.Kind 一致 (暗槓は同種4枚全てを使うため赤ドラ有無で選択肢は生じない)
    /// - 加槓 / 嶺上加槓: 候補の Tile と完全一致 (Tile.Id 一致)。赤ドラ採用/非採用は別候補として列挙される
    /// - チー / ポン / 大明槓: HandTiles が SequenceEqual
    /// - その他 (Ok/Ron/Tsumo/Chankan/Rinshan/Kyuushu): 対応候補が候補リストに存在するだけで可
    /// </summary>
    public static bool IsResponseInCandidates(PlayerResponse response, CandidateList candidates)
    {
        return response switch
        {
            OkResponse => candidates.HasCandidate<OkCandidate>(),
            RonResponse => candidates.HasCandidate<RonCandidate>(),
            TsumoAgariResponse => candidates.HasCandidate<TsumoAgariCandidate>(),
            KyuushuKyuuhaiResponse => candidates.HasCandidate<KyuushuKyuuhaiCandidate>(),
            ChankanRonResponse => candidates.HasCandidate<ChankanRonCandidate>(),
            RinshanTsumoResponse => candidates.HasCandidate<RinshanTsumoAgariCandidate>(),
            DahaiResponse d => MatchesDahai(candidates, d.Tile, d.IsRiichi),
            KanTsumoDahaiResponse d => MatchesDahai(candidates, d.Tile, d.IsRiichi),
            AnkanResponse a => candidates.GetCandidates<AnkanCandidate>().Any(x => x.Tiles.Length > 0 && x.Tiles[0].Kind == a.Tile.Kind),
            KanTsumoAnkanResponse a => candidates.GetCandidates<AnkanCandidate>().Any(x => x.Tiles.Length > 0 && x.Tiles[0].Kind == a.Tile.Kind),
            KakanResponse k => candidates.GetCandidates<KakanCandidate>().Any(x => x.Tile.Equals(k.Tile)),
            KanTsumoKakanResponse k => candidates.GetCandidates<KakanCandidate>().Any(x => x.Tile.Equals(k.Tile)),
            ChiResponse c => candidates.GetCandidates<ChiCandidate>().Any(x => x.HandTiles.SequenceEqual(c.HandTiles)),
            PonResponse p => candidates.GetCandidates<PonCandidate>().Any(x => x.HandTiles.SequenceEqual(p.HandTiles)),
            DaiminkanResponse d => candidates.GetCandidates<DaiminkanCandidate>().Any(x => x.HandTiles.SequenceEqual(d.HandTiles)),
            _ => false,
        };
    }

    private static bool MatchesDahai(CandidateList candidates, Tiles.Tile tile, bool isRiichi)
    {
        return candidates.GetCandidates<DahaiCandidate>()
            .SelectMany(x => x.DahaiOptionList)
            .Any(x => x.Tile.Equals(tile) && (!isRiichi || x.RiichiAvailable));
    }
}
