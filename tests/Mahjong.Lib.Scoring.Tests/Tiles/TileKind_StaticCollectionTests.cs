using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKind_StaticCollectionTests
{
    [Fact]
    public void All_全34種の牌が含まれる()
    {
        // Arrange & Act
        var all = TileKind.All.ToArray();

        // Assert
        Assert.Equal(34, all.Length);
        Assert.Equal(TileKind.Man1, all[0]);
        Assert.Equal(TileKind.Chun, all[33]);
    }

    [Fact]
    public void Numbers_数牌27種が含まれる()
    {
        // Arrange & Act
        var numbers = TileKind.Numbers.ToArray();

        // Assert
        Assert.Equal(27, numbers.Length);
        Assert.All(numbers, x => Assert.True(x.IsNumber));
        Assert.Contains(TileKind.Man1, numbers);
        Assert.Contains(TileKind.Sou9, numbers);
        Assert.DoesNotContain(TileKind.Ton, numbers);
    }

    [Fact]
    public void Mans_萬子9種が含まれる()
    {
        // Arrange & Act
        var mans = TileKind.Mans.ToArray();

        // Assert
        Assert.Equal(9, mans.Length);
        Assert.All(mans, x => Assert.True(x.IsMan));
        Assert.Contains(TileKind.Man1, mans);
        Assert.Contains(TileKind.Man9, mans);
        Assert.DoesNotContain(TileKind.Pin1, mans);
    }

    [Fact]
    public void Pins_筒子9種が含まれる()
    {
        // Arrange & Act
        var pins = TileKind.Pins.ToArray();

        // Assert
        Assert.Equal(9, pins.Length);
        Assert.All(pins, x => Assert.True(x.IsPin));
        Assert.Contains(TileKind.Pin1, pins);
        Assert.Contains(TileKind.Pin9, pins);
        Assert.DoesNotContain(TileKind.Man1, pins);
    }

    [Fact]
    public void Sous_索子9種が含まれる()
    {
        // Arrange & Act
        var sous = TileKind.Sous.ToArray();

        // Assert
        Assert.Equal(9, sous.Length);
        Assert.All(sous, x => Assert.True(x.IsSou));
        Assert.Contains(TileKind.Sou1, sous);
        Assert.Contains(TileKind.Sou9, sous);
        Assert.DoesNotContain(TileKind.Pin1, sous);
    }

    [Fact]
    public void Honors_字牌7種が含まれる()
    {
        // Arrange & Act
        var honors = TileKind.Honors.ToArray();

        // Assert
        Assert.Equal(7, honors.Length);
        Assert.All(honors, x => Assert.True(x.IsHonor));
        Assert.Contains(TileKind.Ton, honors);
        Assert.Contains(TileKind.Chun, honors);
        Assert.DoesNotContain(TileKind.Man1, honors);
    }

    [Fact]
    public void Winds_風牌4種が含まれる()
    {
        // Arrange & Act
        var winds = TileKind.Winds.ToArray();

        // Assert
        Assert.Equal(4, winds.Length);
        Assert.All(winds, x => Assert.True(x.IsWind));
        Assert.Contains(TileKind.Ton, winds);
        Assert.Contains(TileKind.Pei, winds);
        Assert.DoesNotContain(TileKind.Haku, winds);
    }

    [Fact]
    public void Dragons_三元牌3種が含まれる()
    {
        // Arrange & Act
        var dragons = TileKind.Dragons.ToArray();

        // Assert
        Assert.Equal(3, dragons.Length);
        Assert.All(dragons, x => Assert.True(x.IsDragon));
        Assert.Contains(TileKind.Haku, dragons);
        Assert.Contains(TileKind.Chun, dragons);
        Assert.DoesNotContain(TileKind.Ton, dragons);
    }

    [Fact]
    public void Chunchans_中張牌21種が含まれる()
    {
        // Arrange & Act
        var chunchans = TileKind.Chunchans.ToArray();

        // Assert
        Assert.Equal(21, chunchans.Length);
        Assert.All(chunchans, x => Assert.True(x.IsChunchan));
        Assert.Contains(TileKind.Man2, chunchans);
        Assert.Contains(TileKind.Sou8, chunchans);
        Assert.DoesNotContain(TileKind.Man1, chunchans);
        Assert.DoesNotContain(TileKind.Ton, chunchans);
    }

    [Fact]
    public void Yaochus_么九牌13種が含まれる()
    {
        // Arrange & Act
        var yaochus = TileKind.Yaochus.ToArray();

        // Assert
        Assert.Equal(13, yaochus.Length);
        Assert.All(yaochus, x => Assert.True(x.IsYaochu));
        Assert.Contains(TileKind.Man1, yaochus);
        Assert.Contains(TileKind.Man9, yaochus);
        Assert.Contains(TileKind.Ton, yaochus);
        Assert.DoesNotContain(TileKind.Man2, yaochus);
    }

    [Fact]
    public void Routous_老頭牌6種が含まれる()
    {
        // Arrange & Act
        var routous = TileKind.Routous.ToArray();

        // Assert
        Assert.Equal(6, routous.Length);
        Assert.All(routous, x => Assert.True(x.IsRoutou));
        Assert.Contains(TileKind.Man1, routous);
        Assert.Contains(TileKind.Man9, routous);
        Assert.Contains(TileKind.Pin1, routous);
        Assert.Contains(TileKind.Pin9, routous);
        Assert.Contains(TileKind.Sou1, routous);
        Assert.Contains(TileKind.Sou9, routous);
        Assert.DoesNotContain(TileKind.Ton, routous);
        Assert.DoesNotContain(TileKind.Man2, routous);
    }
}
