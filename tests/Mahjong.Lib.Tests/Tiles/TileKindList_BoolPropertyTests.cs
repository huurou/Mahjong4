using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_BoolPropertyTests
{
    [Fact]
    public void IsAllMan_全て萬子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(list.IsAllMan);
    }

    [Fact]
    public void IsAllMan_筒子混在_Falseを返す()
    {
        // Arrange
        var list = new TileKindList(man: "123", pin: "4");

        // Act & Assert
        Assert.False(list.IsAllMan);
    }

    [Fact]
    public void IsAllPin_全て筒子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(pin: "123");

        // Act & Assert
        Assert.True(list.IsAllPin);
    }

    [Fact]
    public void IsAllSou_全て索子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(sou: "123");

        // Act & Assert
        Assert.True(list.IsAllSou);
    }

    [Fact]
    public void IsAllSameSuit_全て同種類_Trueを返す()
    {
        // Arrange
        var manList = new TileKindList(man: "123");
        var pinList = new TileKindList(pin: "456");
        var souList = new TileKindList(sou: "789");

        // Act & Assert
        Assert.True(manList.IsAllSameSuit);
        Assert.True(pinList.IsAllSameSuit);
        Assert.True(souList.IsAllSameSuit);
    }

    [Fact]
    public void IsAllNumber_全て数牌_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "123", pin: "456", sou: "789");

        // Act & Assert
        Assert.True(list.IsAllNumber);
    }

    [Fact]
    public void IsAllHonor_全て字牌_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(honor: "tns");

        // Act & Assert
        Assert.True(list.IsAllHonor);
    }

    [Fact]
    public void IsAllWind_全て風牌_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(honor: "tnsp");

        // Act & Assert
        Assert.True(list.IsAllWind);
    }

    [Fact]
    public void IsAllDragon_全て三元牌_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(honor: "hrc");

        // Act & Assert
        Assert.True(list.IsAllDragon);
    }

    [Fact]
    public void IsToitsu_対子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "11");

        // Act & Assert
        Assert.True(list.IsToitsu);
    }

    [Fact]
    public void IsToitsu_対子でない場合_Falseを返す()
    {
        // Arrange
        var differentList = new TileKindList(man: "12");
        var threeList = new TileKindList(man: "111");

        // Act & Assert
        Assert.False(differentList.IsToitsu);
        Assert.False(threeList.IsToitsu);
    }

    [Fact]
    public void IsShuntsu_順子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(list.IsShuntsu);
    }

    [Fact]
    public void IsShuntsu_順子でない場合_Falseを返す()
    {
        // Arrange
        var skipList = new TileKindList(man: "135");
        var koutsuList = new TileKindList(man: "111");
        var mixedList = new TileKindList(man: "12", pin: "3");

        // Act & Assert
        Assert.False(skipList.IsShuntsu);
        Assert.False(koutsuList.IsShuntsu);
        Assert.False(mixedList.IsShuntsu);
    }

    [Fact]
    public void IsShuntsu_未ソート順子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList([TileKind.Man3, TileKind.Man1, TileKind.Man2]);

        // Act & Assert
        Assert.True(list.IsShuntsu);
    }

    [Fact]
    public void IsKoutsu_刻子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "111");

        // Act & Assert
        Assert.True(list.IsKoutsu);
    }

    [Fact]
    public void IsKoutsu_刻子でない場合_Falseを返す()
    {
        // Arrange
        var differentList = new TileKindList(man: "112");
        var twoList = new TileKindList(man: "11");

        // Act & Assert
        Assert.False(differentList.IsKoutsu);
        Assert.False(twoList.IsKoutsu);
    }

    [Fact]
    public void IsKantsu_槓子_Trueを返す()
    {
        // Arrange
        var list = new TileKindList(man: "1111");

        // Act & Assert
        Assert.True(list.IsKantsu);
    }

    [Fact]
    public void IsKantsu_槓子でない場合_Falseを返す()
    {
        // Arrange
        var differentList = new TileKindList(man: "1112");
        var threeList = new TileKindList(man: "111");

        // Act & Assert
        Assert.False(differentList.IsKantsu);
        Assert.False(threeList.IsKantsu);
    }

    [Fact]
    public void 空リスト_IsAllManからIsAllDragonまで全てTrueを返す()
    {
        // Arrange
        var list = new TileKindList();

        // Act & Assert
        Assert.True(list.IsAllMan);
        Assert.True(list.IsAllPin);
        Assert.True(list.IsAllSou);
        Assert.True(list.IsAllSameSuit);
        Assert.True(list.IsAllNumber);
        Assert.True(list.IsAllHonor);
        Assert.True(list.IsAllWind);
        Assert.True(list.IsAllDragon);
    }

    [Fact]
    public void 空リスト_IsToitsuからIsKantsuまで全てFalseを返す()
    {
        // Arrange
        var list = new TileKindList();

        // Act & Assert
        Assert.False(list.IsToitsu);
        Assert.False(list.IsShuntsu);
        Assert.False(list.IsKoutsu);
        Assert.False(list.IsKantsu);
    }
}
