using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局の統括管理
/// 対局開始・局間引き継ぎ・対局終了判定を担当する
/// </summary>
/// <param name="playerList">プレイヤー席のリスト (index 0 が起家)</param>
/// <param name="rules">対局ルール</param>
/// <param name="wallGenerator">山牌生成機</param>
/// <param name="scoreCalculator">和了時点数計算機</param>
/// <param name="tenpaiChecker">テンパイ判定機</param>
public class GameManager(
    PlayerList playerList,
    GameRules rules,
    IWallGenerator wallGenerator,
    IScoreCalculator scoreCalculator,
    ITenpaiChecker tenpaiChecker
) : IDisposable
{
    private GameStateContext? context_;
    private bool disposed_;

    /// <summary>
    /// 対局状態遷移コンテキスト Start() 呼び出し後に参照可能
    /// </summary>
    public GameStateContext Context => context_
        ?? throw new InvalidOperationException("Start() が呼び出されていません。");

    /// <summary>
    /// 対局を開始します
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (context_ is not null)
        {
            throw new InvalidOperationException("Start() は既に呼び出されています。");
        }

        context_ = new GameStateContext(wallGenerator, scoreCalculator, tenpaiChecker);
        context_.Init(Game.Create(playerList, rules));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed_) { return; }

        if (disposing)
        {
            context_?.Dispose();
        }

        disposed_ = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
