namespace Mahjong.Lib.Game.Players;

/// <summary>
/// Player を席順ごとに生成する抽象。AI バージョン差し替えや自動対局の DI 組み立てで使う。
/// 新バージョンの AI を追加する場合は <c>AI_vX_Y_Z_名前.cs</c> を新規作成し、
/// 末尾に <c>AI_vX_Y_Z_名前Factory</c> を同居させて DI の登録先を差し替えるだけで切り替えられる
/// </summary>
public interface IPlayerFactory
{
    /// <summary>
    /// この Factory が生成する Player の表示名 (AI 識別用、バージョンを含む)
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 指定席に Player を生成する。DisplayName は Factory が自動で付与する
    /// </summary>
    Player Create(PlayerIndex index, PlayerId id);
}
