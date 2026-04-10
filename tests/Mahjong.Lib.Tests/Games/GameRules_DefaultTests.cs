using Mahjong.Lib.Games;

namespace Mahjong.Lib.Tests.Games;

public class GameRules_DefaultTests
{
    [Fact]
    public void デフォルト値_正しく設定される()
    {
        // Arrange & Act
        var rules = new GameRules();

        // Assert
        Assert.True(rules.KuitanEnabled);
        Assert.True(rules.DoubleYakumanEnabled);
        Assert.Equal(KazoeLimit.Limited, rules.KazoeLimit);
        Assert.False(rules.KiriageEnabled);
        Assert.True(rules.PinzumoEnabled);
        Assert.False(rules.RenhouAsYakumanEnabled);
        Assert.False(rules.DaisharinEnabled);
    }
}
