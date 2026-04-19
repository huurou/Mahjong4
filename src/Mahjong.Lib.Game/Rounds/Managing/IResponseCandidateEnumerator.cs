using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// Round と意思決定フェーズから合法な応答候補を列挙する
/// RoundStateContext の通知・応答集約ループが CreateInquirySpec 経由で使用する
/// </summary>
public interface IResponseCandidateEnumerator
{
    /// <summary>
    /// ツモ局面で手番プレイヤーが選択可能な応答候補を列挙する
    /// 打牌/暗槓/加槓/ツモ和了/九種九牌
    /// </summary>
    CandidateList EnumerateForTsumo(Round round, PlayerIndex turnPlayerIndex);

    /// <summary>
    /// 打牌局面で他家が選択可能な応答候補を列挙する
    /// チー/ポン/大明槓/ロン/OK (スルー)
    /// </summary>
    CandidateList EnumerateForDahai(Round round, PlayerIndex responderIndex, Tile discardedTile);

    /// <summary>
    /// 槓 (暗槓/加槓) 局面で他家が選択可能な応答候補を列挙する
    /// 加槓: 槍槓ロン / OK (スルー)
    /// 暗槓: 国士テンパイ時のみロン候補を提示する (<see cref="Games.GameRules.AllowAnkanChankanForKokushi"/> = true の場合)
    /// kanTiles には槓子4枚を渡す (暗槓: 手牌から引き抜いた4枚 / 加槓: 元ポン3枚 + 追加牌1枚)
    /// </summary>
    CandidateList EnumerateForKan(Round round, PlayerIndex responderIndex, ImmutableArray<Tile> kanTiles, CallType kanType);

    /// <summary>
    /// 嶺上ツモ局面で手番プレイヤーが選択可能な応答候補を列挙する
    /// 嶺上ツモ和了/打牌/暗槓/加槓
    /// </summary>
    CandidateList EnumerateForKanTsumo(Round round, PlayerIndex turnPlayerIndex);

    /// <summary>
    /// 嶺上ツモ後 (ツモ和了しなかった場合) に手番プレイヤーが選択可能な応答候補を列挙する
    /// 打牌/暗槓/加槓
    /// </summary>
    CandidateList EnumerateForAfterKanTsumo(Round round, PlayerIndex turnPlayerIndex);
}
