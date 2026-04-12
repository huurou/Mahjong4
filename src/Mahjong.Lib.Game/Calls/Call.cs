using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Calls;

public record Call(
    CallType Type,
    TileList TileList,
    PlayerIndex From,
    Tile CalledTile
);
