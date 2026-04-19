namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// プレイヤー通知の一意識別子 (PN_uuidv7 形式)
/// </summary>
public record NotificationId
{
    private const string PREFIX = "PN";

    public string Value { get; }

    public NotificationId(string value)
    {
        IdUtility.ValidateId(value, PREFIX);

        Value = value;
    }

    public static NotificationId NewId()
    {
        return new NotificationId(IdUtility.NewId(PREFIX));
    }
}
