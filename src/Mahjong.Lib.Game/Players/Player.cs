namespace Mahjong.Lib.Game.Players;

/// <summary>
/// プレイヤー
/// Phase 1 では識別情報 (PlayerId / DisplayName) のみ Phase 4 で通知・応答メソッドを追加する
/// </summary>
/// <param name="PlayerId">プレイヤー識別子</param>
/// <param name="DisplayName">表示名</param>
public abstract record Player(PlayerId PlayerId, string DisplayName);
