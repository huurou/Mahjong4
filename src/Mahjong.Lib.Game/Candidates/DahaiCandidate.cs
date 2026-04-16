namespace Mahjong.Lib.Game.Candidates;

/// <summary>
/// 打牌応答候補 手番プレイヤーが打牌可能な牌と立直可否を提示する
/// </summary>
/// <param name="DahaiOptionList">打牌可能な牌ごとの選択肢</param>
public record DahaiCandidate(DahaiOptionList DahaiOptionList) : ResponseCandidate;
