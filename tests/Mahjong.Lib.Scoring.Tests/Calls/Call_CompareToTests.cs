using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class Call_CompareToTests
{
    [Fact]
    public void 同じCallType同じTileKindList_0を返す()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var call2 = Call.Chi(new TileKindList(man: "123"));

        // Act
        var result = call1.CompareTo(call2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CallTypeが異なる場合_CallTypeの順序で比較される()
    {
        // Arrange
        var chi = Call.Chi(new TileKindList(man: "123"));
        var pon = Call.Pon(new TileKindList(man: "111"));

        // Act & Assert
        Assert.True(chi.CompareTo(pon) < 0);
        Assert.True(pon.CompareTo(chi) > 0);
    }

    [Fact]
    public void 同じCallTypeでTileKindListが異なる場合_TileKindListの順序で比較される()
    {
        // Arrange
        var chi1 = Call.Chi(new TileKindList(man: "123"));
        var chi2 = Call.Chi(new TileKindList(man: "234"));

        // Act & Assert
        Assert.True(chi1.CompareTo(chi2) < 0);
        Assert.True(chi2.CompareTo(chi1) > 0);
    }

    [Fact]
    public void Nullと比較_正の値を返す()
    {
        // Arrange
        var call = Call.Chi(new TileKindList(man: "123"));

        // Act
        var result = call.CompareTo(null);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void 比較演算子_正しく動作する()
    {
        // Arrange
        var chi = Call.Chi(new TileKindList(man: "123"));
        var pon = Call.Pon(new TileKindList(man: "111"));
        var chi2 = Call.Chi(new TileKindList(man: "123"));

        // Act & Assert
        Assert.True(chi < pon);
        Assert.True(pon > chi);
        Assert.True(chi <= chi2);
        Assert.True(chi >= chi2);
        Assert.False(chi > pon);
        Assert.False(pon < chi);
    }

    [Fact]
    public void 比較演算子_leftがnull_正しく動作する()
    {
        // Arrange
        Call? nullCall = null;
        var call = Call.Chi(new TileKindList(man: "123"));

        // Act & Assert
        Assert.True(nullCall < call);
        Assert.False(nullCall > call);
        Assert.True(nullCall <= call);
        Assert.False(nullCall >= call);
    }

    [Fact]
    public void 比較演算子_両方null_正しく動作する()
    {
        // Arrange
        Call? left = null;
        Call? right = null;

        // Act & Assert
        Assert.False(left < right);
        Assert.False(left > right);
        Assert.True(left <= right);
        Assert.True(left >= right);
    }
}
