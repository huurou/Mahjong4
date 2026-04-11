using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Tests.Games;

public class WinSituation_DefaultTests
{
    [Fact]
    public void デフォルト値_正しく設定される()
    {
        // Arrange & Act
        var situation = new WinSituation();

        // Assert
        Assert.False(situation.IsTsumo);
        Assert.False(situation.IsRiichi);
        Assert.False(situation.IsIppatsu);
        Assert.False(situation.IsChankan);
        Assert.False(situation.IsRinshan);
        Assert.False(situation.IsHaitei);
        Assert.False(situation.IsHoutei);
        Assert.False(situation.IsDoubleRiichi);
        Assert.False(situation.IsNagashimangan);
        Assert.False(situation.IsTenhou);
        Assert.False(situation.IsChiihou);
        Assert.False(situation.IsRenhou);
        Assert.Equal(Wind.East, situation.PlayerWind);
        Assert.Equal(Wind.East, situation.RoundWind);
        Assert.Equal(0, situation.AkadoraCount);
        Assert.True(situation.IsDealer);
    }
}
