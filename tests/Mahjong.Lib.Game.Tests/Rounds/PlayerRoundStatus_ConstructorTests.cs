using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class PlayerRoundStatus_ConstructorTests
{
    [Fact]
    public void 既定値_門前かつ第一打前かつ流し満貫条件あり()
    {
        // Act
        var status = new PlayerRoundStatus();

        // Assert
        Assert.True(status.IsMenzen);
        Assert.True(status.IsFirstTurnBeforeDiscard);
        Assert.True(status.IsNagashiMangan);
        Assert.False(status.IsRiichi);
        Assert.False(status.IsDoubleRiichi);
        Assert.False(status.IsIppatsu);
        Assert.False(status.IsRinshan);
        Assert.False(status.IsTemporaryFuriten);
        Assert.False(status.IsFuriten);
    }

    [Fact]
    public void 立直宣言後の状態_指定値が保持される()
    {
        // Act
        var status = new PlayerRoundStatus(IsRiichi: true, IsIppatsu: true, IsFirstTurnBeforeDiscard: false);

        // Assert
        Assert.True(status.IsRiichi);
        Assert.True(status.IsIppatsu);
        Assert.False(status.IsFirstTurnBeforeDiscard);
        Assert.True(status.IsMenzen);
    }
}
