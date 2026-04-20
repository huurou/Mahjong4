using GameCall = Mahjong.Lib.Game.Calls.Call;
using GameCallList = Mahjong.Lib.Game.Calls.CallList;
using GameCallType = Mahjong.Lib.Game.Calls.CallType;
using ScoringCall = Mahjong.Lib.Scoring.Calls.Call;
using ScoringCallList = Mahjong.Lib.Scoring.Calls.CallList;
using ScoringCallType = Mahjong.Lib.Scoring.Calls.CallType;

namespace Mahjong.Lib.Game.Scoring.Conversions;

internal static class CallConverter
{
    public static ScoringCall ToScoringCall(this GameCall call)
    {
        var type = call.Type switch
        {
            GameCallType.Chi => ScoringCallType.Chi,
            GameCallType.Pon => ScoringCallType.Pon,
            GameCallType.Ankan => ScoringCallType.Ankan,
            GameCallType.Daiminkan => ScoringCallType.Minkan,
            GameCallType.Kakan => ScoringCallType.Minkan,
            _ => throw new ArgumentOutOfRangeException(nameof(call), call.Type, "未対応の副露種別です。"),
        };
        return new ScoringCall(type, call.Tiles.ToTileKindList());
    }

    public static ScoringCallList ToScoringCallList(this GameCallList callList)
    {
        return [.. callList.Select(ToScoringCall)];
    }
}
