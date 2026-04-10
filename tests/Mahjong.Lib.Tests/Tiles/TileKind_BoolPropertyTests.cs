using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_BoolPropertyTests
{
    [Fact]
    public void IsNumber_数牌ならtrue字牌ならfalse()
    {
        // Arrange & Act & Assert
        // 萬子
        Assert.True(TileKind.Man1.IsNumber);
        Assert.True(TileKind.Man2.IsNumber);
        Assert.True(TileKind.Man3.IsNumber);
        Assert.True(TileKind.Man4.IsNumber);
        Assert.True(TileKind.Man5.IsNumber);
        Assert.True(TileKind.Man6.IsNumber);
        Assert.True(TileKind.Man7.IsNumber);
        Assert.True(TileKind.Man8.IsNumber);
        Assert.True(TileKind.Man9.IsNumber);

        // 筒子
        Assert.True(TileKind.Pin1.IsNumber);
        Assert.True(TileKind.Pin2.IsNumber);
        Assert.True(TileKind.Pin3.IsNumber);
        Assert.True(TileKind.Pin4.IsNumber);
        Assert.True(TileKind.Pin5.IsNumber);
        Assert.True(TileKind.Pin6.IsNumber);
        Assert.True(TileKind.Pin7.IsNumber);
        Assert.True(TileKind.Pin8.IsNumber);
        Assert.True(TileKind.Pin9.IsNumber);

        // 索子
        Assert.True(TileKind.Sou1.IsNumber);
        Assert.True(TileKind.Sou2.IsNumber);
        Assert.True(TileKind.Sou3.IsNumber);
        Assert.True(TileKind.Sou4.IsNumber);
        Assert.True(TileKind.Sou5.IsNumber);
        Assert.True(TileKind.Sou6.IsNumber);
        Assert.True(TileKind.Sou7.IsNumber);
        Assert.True(TileKind.Sou8.IsNumber);
        Assert.True(TileKind.Sou9.IsNumber);

        // 字牌
        Assert.False(TileKind.Ton.IsNumber);
        Assert.False(TileKind.Nan.IsNumber);
        Assert.False(TileKind.Sha.IsNumber);
        Assert.False(TileKind.Pei.IsNumber);
        Assert.False(TileKind.Haku.IsNumber);
        Assert.False(TileKind.Hatsu.IsNumber);
        Assert.False(TileKind.Chun.IsNumber);
    }

    [Fact]
    public void IsMan_萬子ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Man1.IsMan);
        Assert.True(TileKind.Man2.IsMan);
        Assert.True(TileKind.Man3.IsMan);
        Assert.True(TileKind.Man4.IsMan);
        Assert.True(TileKind.Man5.IsMan);
        Assert.True(TileKind.Man6.IsMan);
        Assert.True(TileKind.Man7.IsMan);
        Assert.True(TileKind.Man8.IsMan);
        Assert.True(TileKind.Man9.IsMan);

        Assert.False(TileKind.Pin1.IsMan);
        Assert.False(TileKind.Pin9.IsMan);
        Assert.False(TileKind.Sou1.IsMan);
        Assert.False(TileKind.Sou9.IsMan);
        Assert.False(TileKind.Ton.IsMan);
        Assert.False(TileKind.Chun.IsMan);
    }

    [Fact]
    public void IsPin_筒子ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Pin1.IsPin);
        Assert.True(TileKind.Pin2.IsPin);
        Assert.True(TileKind.Pin3.IsPin);
        Assert.True(TileKind.Pin4.IsPin);
        Assert.True(TileKind.Pin5.IsPin);
        Assert.True(TileKind.Pin6.IsPin);
        Assert.True(TileKind.Pin7.IsPin);
        Assert.True(TileKind.Pin8.IsPin);
        Assert.True(TileKind.Pin9.IsPin);

        Assert.False(TileKind.Man1.IsPin);
        Assert.False(TileKind.Man9.IsPin);
        Assert.False(TileKind.Sou1.IsPin);
        Assert.False(TileKind.Sou9.IsPin);
        Assert.False(TileKind.Ton.IsPin);
        Assert.False(TileKind.Chun.IsPin);
    }

    [Fact]
    public void IsSou_索子ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Sou1.IsSou);
        Assert.True(TileKind.Sou2.IsSou);
        Assert.True(TileKind.Sou3.IsSou);
        Assert.True(TileKind.Sou4.IsSou);
        Assert.True(TileKind.Sou5.IsSou);
        Assert.True(TileKind.Sou6.IsSou);
        Assert.True(TileKind.Sou7.IsSou);
        Assert.True(TileKind.Sou8.IsSou);
        Assert.True(TileKind.Sou9.IsSou);

        Assert.False(TileKind.Man1.IsSou);
        Assert.False(TileKind.Man9.IsSou);
        Assert.False(TileKind.Pin1.IsSou);
        Assert.False(TileKind.Pin9.IsSou);
        Assert.False(TileKind.Ton.IsSou);
        Assert.False(TileKind.Chun.IsSou);
    }

    [Fact]
    public void IsHonor_字牌ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Ton.IsHonor);
        Assert.True(TileKind.Nan.IsHonor);
        Assert.True(TileKind.Sha.IsHonor);
        Assert.True(TileKind.Pei.IsHonor);
        Assert.True(TileKind.Haku.IsHonor);
        Assert.True(TileKind.Hatsu.IsHonor);
        Assert.True(TileKind.Chun.IsHonor);

        Assert.False(TileKind.Man1.IsHonor);
        Assert.False(TileKind.Man9.IsHonor);
        Assert.False(TileKind.Pin1.IsHonor);
        Assert.False(TileKind.Pin9.IsHonor);
        Assert.False(TileKind.Sou1.IsHonor);
        Assert.False(TileKind.Sou9.IsHonor);
    }

    [Fact]
    public void IsWind_風牌ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Ton.IsWind);
        Assert.True(TileKind.Nan.IsWind);
        Assert.True(TileKind.Sha.IsWind);
        Assert.True(TileKind.Pei.IsWind);

        Assert.False(TileKind.Haku.IsWind);
        Assert.False(TileKind.Hatsu.IsWind);
        Assert.False(TileKind.Chun.IsWind);
        Assert.False(TileKind.Man1.IsWind);
        Assert.False(TileKind.Sou9.IsWind);
    }

    [Fact]
    public void IsDragon_三元牌ならtrue()
    {
        // Arrange & Act & Assert
        Assert.True(TileKind.Haku.IsDragon);
        Assert.True(TileKind.Hatsu.IsDragon);
        Assert.True(TileKind.Chun.IsDragon);

        Assert.False(TileKind.Ton.IsDragon);
        Assert.False(TileKind.Nan.IsDragon);
        Assert.False(TileKind.Sha.IsDragon);
        Assert.False(TileKind.Pei.IsDragon);
        Assert.False(TileKind.Man1.IsDragon);
        Assert.False(TileKind.Sou9.IsDragon);
    }

    [Fact]
    public void IsChunchan_中張牌ならtrue()
    {
        // Arrange & Act & Assert
        // 萬子
        Assert.False(TileKind.Man1.IsChunchan);
        Assert.True(TileKind.Man2.IsChunchan);
        Assert.True(TileKind.Man3.IsChunchan);
        Assert.True(TileKind.Man4.IsChunchan);
        Assert.True(TileKind.Man5.IsChunchan);
        Assert.True(TileKind.Man6.IsChunchan);
        Assert.True(TileKind.Man7.IsChunchan);
        Assert.True(TileKind.Man8.IsChunchan);
        Assert.False(TileKind.Man9.IsChunchan);

        // 筒子
        Assert.False(TileKind.Pin1.IsChunchan);
        Assert.True(TileKind.Pin2.IsChunchan);
        Assert.True(TileKind.Pin3.IsChunchan);
        Assert.True(TileKind.Pin4.IsChunchan);
        Assert.True(TileKind.Pin5.IsChunchan);
        Assert.True(TileKind.Pin6.IsChunchan);
        Assert.True(TileKind.Pin7.IsChunchan);
        Assert.True(TileKind.Pin8.IsChunchan);
        Assert.False(TileKind.Pin9.IsChunchan);

        // 索子
        Assert.False(TileKind.Sou1.IsChunchan);
        Assert.True(TileKind.Sou2.IsChunchan);
        Assert.True(TileKind.Sou3.IsChunchan);
        Assert.True(TileKind.Sou4.IsChunchan);
        Assert.True(TileKind.Sou5.IsChunchan);
        Assert.True(TileKind.Sou6.IsChunchan);
        Assert.True(TileKind.Sou7.IsChunchan);
        Assert.True(TileKind.Sou8.IsChunchan);
        Assert.False(TileKind.Sou9.IsChunchan);

        // 字牌
        Assert.False(TileKind.Ton.IsChunchan);
        Assert.False(TileKind.Nan.IsChunchan);
        Assert.False(TileKind.Sha.IsChunchan);
        Assert.False(TileKind.Pei.IsChunchan);
        Assert.False(TileKind.Haku.IsChunchan);
        Assert.False(TileKind.Hatsu.IsChunchan);
        Assert.False(TileKind.Chun.IsChunchan);
    }

    [Fact]
    public void IsYaochu_么九牌ならtrue()
    {
        // Arrange & Act & Assert
        // 萬子
        Assert.True(TileKind.Man1.IsYaochu);
        Assert.False(TileKind.Man2.IsYaochu);
        Assert.False(TileKind.Man3.IsYaochu);
        Assert.False(TileKind.Man4.IsYaochu);
        Assert.False(TileKind.Man5.IsYaochu);
        Assert.False(TileKind.Man6.IsYaochu);
        Assert.False(TileKind.Man7.IsYaochu);
        Assert.False(TileKind.Man8.IsYaochu);
        Assert.True(TileKind.Man9.IsYaochu);

        // 筒子
        Assert.True(TileKind.Pin1.IsYaochu);
        Assert.False(TileKind.Pin2.IsYaochu);
        Assert.False(TileKind.Pin3.IsYaochu);
        Assert.False(TileKind.Pin4.IsYaochu);
        Assert.False(TileKind.Pin5.IsYaochu);
        Assert.False(TileKind.Pin6.IsYaochu);
        Assert.False(TileKind.Pin7.IsYaochu);
        Assert.False(TileKind.Pin8.IsYaochu);
        Assert.True(TileKind.Pin9.IsYaochu);

        // 索子
        Assert.True(TileKind.Sou1.IsYaochu);
        Assert.False(TileKind.Sou2.IsYaochu);
        Assert.False(TileKind.Sou3.IsYaochu);
        Assert.False(TileKind.Sou4.IsYaochu);
        Assert.False(TileKind.Sou5.IsYaochu);
        Assert.False(TileKind.Sou6.IsYaochu);
        Assert.False(TileKind.Sou7.IsYaochu);
        Assert.False(TileKind.Sou8.IsYaochu);
        Assert.True(TileKind.Sou9.IsYaochu);

        // 字牌
        Assert.True(TileKind.Ton.IsYaochu);
        Assert.True(TileKind.Nan.IsYaochu);
        Assert.True(TileKind.Sha.IsYaochu);
        Assert.True(TileKind.Pei.IsYaochu);
        Assert.True(TileKind.Haku.IsYaochu);
        Assert.True(TileKind.Hatsu.IsYaochu);
        Assert.True(TileKind.Chun.IsYaochu);
    }

    [Fact]
    public void IsRoutou_老頭牌ならtrue()
    {
        // Arrange & Act & Assert
        // 萬子
        Assert.True(TileKind.Man1.IsRoutou);
        Assert.False(TileKind.Man2.IsRoutou);
        Assert.False(TileKind.Man3.IsRoutou);
        Assert.False(TileKind.Man4.IsRoutou);
        Assert.False(TileKind.Man5.IsRoutou);
        Assert.False(TileKind.Man6.IsRoutou);
        Assert.False(TileKind.Man7.IsRoutou);
        Assert.False(TileKind.Man8.IsRoutou);
        Assert.True(TileKind.Man9.IsRoutou);

        // 筒子
        Assert.True(TileKind.Pin1.IsRoutou);
        Assert.False(TileKind.Pin2.IsRoutou);
        Assert.False(TileKind.Pin3.IsRoutou);
        Assert.False(TileKind.Pin4.IsRoutou);
        Assert.False(TileKind.Pin5.IsRoutou);
        Assert.False(TileKind.Pin6.IsRoutou);
        Assert.False(TileKind.Pin7.IsRoutou);
        Assert.False(TileKind.Pin8.IsRoutou);
        Assert.True(TileKind.Pin9.IsRoutou);

        // 索子
        Assert.True(TileKind.Sou1.IsRoutou);
        Assert.False(TileKind.Sou2.IsRoutou);
        Assert.False(TileKind.Sou3.IsRoutou);
        Assert.False(TileKind.Sou4.IsRoutou);
        Assert.False(TileKind.Sou5.IsRoutou);
        Assert.False(TileKind.Sou6.IsRoutou);
        Assert.False(TileKind.Sou7.IsRoutou);
        Assert.False(TileKind.Sou8.IsRoutou);
        Assert.True(TileKind.Sou9.IsRoutou);

        // 字牌
        Assert.False(TileKind.Ton.IsRoutou);
        Assert.False(TileKind.Nan.IsRoutou);
        Assert.False(TileKind.Sha.IsRoutou);
        Assert.False(TileKind.Pei.IsRoutou);
        Assert.False(TileKind.Haku.IsRoutou);
        Assert.False(TileKind.Hatsu.IsRoutou);
        Assert.False(TileKind.Chun.IsRoutou);
    }
}
