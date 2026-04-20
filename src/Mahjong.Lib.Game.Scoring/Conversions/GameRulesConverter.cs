using GameRulesGame = Mahjong.Lib.Game.Games.GameRules;
using GameRulesScoring = Mahjong.Lib.Scoring.Games.GameRules;

namespace Mahjong.Lib.Game.Scoring.Conversions;

internal static class GameRulesConverter
{
    public static GameRulesScoring ToScoringGameRules(this GameRulesGame rules)
    {
        return new GameRulesScoring
        {
            KuitanEnabled = rules.KuitanAllowed,
        };
    }
}
