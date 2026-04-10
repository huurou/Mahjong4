using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_ToStringTests
{
    [Fact]
    public void 各符の文字列表現を返す()
    {
        // Arrange & Act & Assert
        Assert.Equal("副底:20符", Fu.Futei.ToString());
        Assert.Equal("面前加符:10符", Fu.Menzen.ToString());
        Assert.Equal("七対子符:25符", Fu.Chiitoitsu.ToString());
        Assert.Equal("副底(食い平和):30符", Fu.FuteiOpenPinfu.ToString());
        Assert.Equal("ツモ符:2符", Fu.Tsumo.ToString());
        Assert.Equal("カンチャン待ち:2符", Fu.Kanchan.ToString());
        Assert.Equal("ペンチャン待ち:2符", Fu.Penchan.ToString());
        Assert.Equal("単騎待ち:2符", Fu.Tanki.ToString());
        Assert.Equal("自風の雀頭:2符", Fu.JantouPlayerWind.ToString());
        Assert.Equal("場風の雀頭:2符", Fu.JantouRoundWind.ToString());
        Assert.Equal("三元牌の雀頭:2符", Fu.JantouDragon.ToString());
        Assert.Equal("中張牌の明刻:2符", Fu.MinkoChunchan.ToString());
        Assert.Equal("么九牌の明刻:4符", Fu.MinkoYaochu.ToString());
        Assert.Equal("中張牌の暗刻:4符", Fu.AnkoChunchan.ToString());
        Assert.Equal("么九牌の暗刻:8符", Fu.AnkoYaochu.ToString());
        Assert.Equal("中張牌の明槓:8符", Fu.MinkanChunchan.ToString());
        Assert.Equal("么九牌の明槓:16符", Fu.MinkanYaochu.ToString());
        Assert.Equal("中張牌の暗槓:16符", Fu.AnkanChunchan.ToString());
        Assert.Equal("么九牌の暗槓:32符", Fu.AnkanYaochu.ToString());
    }
}
