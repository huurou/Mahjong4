using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundEventResponseCall_ConstructorTests
{
    [Fact]
    public void ポン_同牌種3枚で正常に作成される()
    {
        // Arrange
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1));
        var calledTile = new Tile(2);

        // Act
        var evt = new RoundEventResponseCall(new PlayerIndex(0), CallType.Pon, handTiles, calledTile);

        // Assert
        Assert.Equal(CallType.Pon, evt.CallType);
        Assert.Equal(calledTile, evt.CalledTile);
    }

    [Fact]
    public void 大明槓_同牌種4枚で正常に作成される()
    {
        // Arrange
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2));
        var calledTile = new Tile(3);

        // Act
        var evt = new RoundEventResponseCall(new PlayerIndex(0), CallType.Daiminkan, handTiles, calledTile);

        // Assert
        Assert.Equal(CallType.Daiminkan, evt.CallType);
    }

    [Fact]
    public void チー_連続する数牌3枚で正常に作成される()
    {
        // Arrange (1m, 2m + 3m)
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(4));
        var calledTile = new Tile(8);

        // Act
        var evt = new RoundEventResponseCall(new PlayerIndex(0), CallType.Chi, handTiles, calledTile);

        // Assert
        Assert.Equal(CallType.Chi, evt.CallType);
    }

    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Kakan)]
    public void 槓種別を指定_ArgumentExceptionが発生する(CallType type)
    {
        // Arrange
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1));

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), type, handTiles, new Tile(2)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ポン_calledTileが異なる牌種_ArgumentExceptionが発生する()
    {
        // Arrange
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1));
        var calledTile = new Tile(4); // 異なる牌種

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), CallType.Pon, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void チー_連続しない_ArgumentExceptionが発生する()
    {
        // Arrange (1m, 2m + 4m)
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(4));
        var calledTile = new Tile(12);

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), CallType.Chi, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Theory]
    [InlineData(CallType.Chi)]
    [InlineData(CallType.Pon)]
    public void ChiまたはPon_handTilesが1枚_ArgumentExceptionが発生する(CallType type)
    {
        // Arrange (handTilesは2枚必要なのに1枚)
        var handTiles = ImmutableList.Create(new Tile(0));
        var calledTile = new Tile(1);

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), type, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 大明槓_handTilesが2枚_ArgumentExceptionが発生する()
    {
        // Arrange (handTilesは3枚必要なのに2枚)
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1));
        var calledTile = new Tile(2);

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), CallType.Daiminkan, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ポン_handTilesにcalledTileと同じ物理牌を含む_ArgumentExceptionが発生する()
    {
        // Arrange (同じTile(0)が自分の手にある状態は不正)
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1));
        var calledTile = new Tile(0);

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), CallType.Pon, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 大明槓_handTilesにcalledTileと同じ物理牌を含む_ArgumentExceptionが発生する()
    {
        // Arrange
        var handTiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2));
        var calledTile = new Tile(0);

        // Act
        var ex = Record.Exception(() => new RoundEventResponseCall(new PlayerIndex(0), CallType.Daiminkan, handTiles, calledTile));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
