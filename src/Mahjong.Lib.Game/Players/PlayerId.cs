namespace Mahjong.Lib.Game.Players;

public record PlayerId
{
    private const string PREFIX = "PLAYER";

    public string Value { get; }

    public PlayerId(string value)
    {
        IdUtility.ValidateId(value, PREFIX);

        Value = value;
    }

    public static PlayerId NewId()
    {
        return new PlayerId(IdUtility.NewId(PREFIX));
    }
}
