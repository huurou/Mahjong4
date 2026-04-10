using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class Call_BoolPropertyTests
{
    [Fact]
    public void IsChi_チーの場合_Trueを返す()
    {
        // Arrange
        var call = Call.Chi(new TileKindList(man: "123"));

        // Act & Assert
        Assert.True(call.IsChi);
    }

    [Fact]
    public void IsChi_チー以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Pon(new TileKindList(man: "111")).IsChi);
        Assert.False(Call.Ankan(new TileKindList(man: "1111")).IsChi);
        Assert.False(Call.Minkan(new TileKindList(man: "1111")).IsChi);
        Assert.False(Call.Nuki(new TileKindList(man: "1")).IsChi);
    }

    [Fact]
    public void IsPon_ポンの場合_Trueを返す()
    {
        // Arrange
        var call = Call.Pon(new TileKindList(man: "111"));

        // Act & Assert
        Assert.True(call.IsPon);
    }

    [Fact]
    public void IsPon_ポン以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Chi(new TileKindList(man: "123")).IsPon);
        Assert.False(Call.Ankan(new TileKindList(man: "1111")).IsPon);
        Assert.False(Call.Minkan(new TileKindList(man: "1111")).IsPon);
        Assert.False(Call.Nuki(new TileKindList(man: "1")).IsPon);
    }

    [Fact]
    public void IsKan_槓の場合_Trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Call.Ankan(new TileKindList(man: "1111")).IsKan);
        Assert.True(Call.Minkan(new TileKindList(man: "1111")).IsKan);
    }

    [Fact]
    public void IsKan_槓以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Chi(new TileKindList(man: "123")).IsKan);
        Assert.False(Call.Pon(new TileKindList(man: "111")).IsKan);
        Assert.False(Call.Nuki(new TileKindList(man: "1")).IsKan);
    }

    [Fact]
    public void IsAnkan_暗槓の場合_Trueを返す()
    {
        // Arrange
        var call = Call.Ankan(new TileKindList(man: "1111"));

        // Act & Assert
        Assert.True(call.IsAnkan);
    }

    [Fact]
    public void IsAnkan_暗槓以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Chi(new TileKindList(man: "123")).IsAnkan);
        Assert.False(Call.Pon(new TileKindList(man: "111")).IsAnkan);
        Assert.False(Call.Minkan(new TileKindList(man: "1111")).IsAnkan);
        Assert.False(Call.Nuki(new TileKindList(man: "1")).IsAnkan);
    }

    [Fact]
    public void IsMinkan_明槓の場合_Trueを返す()
    {
        // Arrange
        var call = Call.Minkan(new TileKindList(man: "1111"));

        // Act & Assert
        Assert.True(call.IsMinkan);
    }

    [Fact]
    public void IsMinkan_明槓以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Chi(new TileKindList(man: "123")).IsMinkan);
        Assert.False(Call.Pon(new TileKindList(man: "111")).IsMinkan);
        Assert.False(Call.Ankan(new TileKindList(man: "1111")).IsMinkan);
        Assert.False(Call.Nuki(new TileKindList(man: "1")).IsMinkan);
    }

    [Fact]
    public void IsNuki_抜きの場合_Trueを返す()
    {
        // Arrange
        var call = Call.Nuki(new TileKindList(man: "1"));

        // Act & Assert
        Assert.True(call.IsNuki);
    }

    [Fact]
    public void IsNuki_抜き以外の場合_Falseを返す()
    {
        // Arrange & Act & Assert
        Assert.False(Call.Chi(new TileKindList(man: "123")).IsNuki);
        Assert.False(Call.Pon(new TileKindList(man: "111")).IsNuki);
        Assert.False(Call.Ankan(new TileKindList(man: "1111")).IsNuki);
        Assert.False(Call.Minkan(new TileKindList(man: "1111")).IsNuki);
    }

    [Fact]
    public void IsOpen_開いている副露の場合_Trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Call.Chi(new TileKindList(man: "123")).IsOpen);
        Assert.True(Call.Pon(new TileKindList(man: "111")).IsOpen);
        Assert.True(Call.Minkan(new TileKindList(man: "1111")).IsOpen);
        Assert.True(Call.Nuki(new TileKindList(man: "1")).IsOpen);
    }

    [Fact]
    public void IsOpen_暗槓の場合_Falseを返す()
    {
        // Arrange
        var call = Call.Ankan(new TileKindList(man: "1111"));

        // Act & Assert
        Assert.False(call.IsOpen);
    }
}
