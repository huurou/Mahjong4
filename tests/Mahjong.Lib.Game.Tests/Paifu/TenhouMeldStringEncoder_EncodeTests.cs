using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouMeldStringEncoder_EncodeTests
{
    private static readonly GameRules rules_ = new();

    [Fact]
    public void チー_1m2m3m_上家1mをチー_c111213()
    {
        var caller = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        var call = new Call(CallType.Chi, [new Tile(0), new Tile(4), new Tile(8)], from, new Tile(0));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("c111213", result);
    }

    [Fact]
    public void ポン_上家1pを鳴く_p212121()
    {
        var caller = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        var call = new Call(CallType.Pon, [new Tile(36), new Tile(37), new Tile(38)], from, new Tile(36));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("p212121", result);
    }

    [Fact]
    public void ポン_対面2sを鳴く_31p3131()
    {
        var caller = new PlayerIndex(0);
        var from = new PlayerIndex(2);
        var call = new Call(CallType.Pon, [new Tile(72), new Tile(73), new Tile(74)], from, new Tile(72));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        // 対面 = fromRel 2 → "{t1}p{called}{t2}"
        Assert.Equal("31p3131", result);
    }

    [Fact]
    public void ポン_下家3pを鳴く_3131p31()
    {
        var caller = new PlayerIndex(0);
        var from = new PlayerIndex(1);
        var call = new Call(CallType.Pon, [new Tile(36), new Tile(37), new Tile(38)], from, new Tile(36));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        // 下家 = fromRel 1 → "{t1}{t2}p{called}"
        Assert.Equal("2121p21", result);
    }

    [Fact]
    public void 大明槓_上家1sを鳴く_m31313131()
    {
        var caller = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        var call = new Call(CallType.Daiminkan, [new Tile(72), new Tile(73), new Tile(74), new Tile(75)], from, new Tile(72));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("m31313131", result);
    }

    [Fact]
    public void 暗槓_9p4枚_292929a29()
    {
        var caller = new PlayerIndex(2);
        var call = new Call(CallType.Ankan, [new Tile(68), new Tile(69), new Tile(70), new Tile(71)], caller, null);

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("292929a29", result);
    }

    [Fact]
    public void 暗槓_5m3枚と赤5m_151515a51()
    {
        var caller = new PlayerIndex(2);
        // 5m の Id は 16-19、16 が赤 → 末尾に赤を置き先頭 3 枚は通常 5m
        var call = new Call(CallType.Ankan, [new Tile(16), new Tile(17), new Tile(18), new Tile(19)], caller, null);

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("151515a51", result);
    }

    [Fact]
    public void 加槓_上家1pポンに加槓_k21212121()
    {
        var caller = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        // 元ポン Tiles [36, 37, 38] + 加槓追加 39
        var call = new Call(CallType.Kakan, [new Tile(36), new Tile(37), new Tile(38), new Tile(39)], from, new Tile(36));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("k21212121", result);
    }

    [Fact]
    public void 加槓_対面3pポンに加槓_23k232323()
    {
        var caller = new PlayerIndex(0);
        var from = new PlayerIndex(2);
        var call = new Call(CallType.Kakan, [new Tile(44), new Tile(45), new Tile(46), new Tile(47)], from, new Tile(44));

        var result = TenhouMeldStringEncoder.Encode(call, caller, rules_);

        Assert.Equal("23k232323", result);
    }
}
