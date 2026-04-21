using System.Security.Cryptography;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// 自動対局オプション
/// </summary>
/// <param name="GameCount">対局数</param>
/// <param name="Seed">乱数シード (IWallGenerator / AI 共通)</param>
/// <param name="OutputDirectory">牌譜出力先ディレクトリ</param>
/// <param name="WritePaifu">牌譜を書き出すか</param>
/// <param name="Parallelism">並列 worker 数。0 以下を指定すると <see cref="Environment.ProcessorCount"/> を使用 (論理コア自動検出)。既定値は 4 (ベンチマークで最もスループットが高かった設定)</param>
public record AutoPlayOptions(int GameCount, int Seed, string OutputDirectory, bool WritePaifu, int Parallelism)
{
    public static AutoPlayOptions Parse(string[] args)
    {
        var gameCount = 20;
        var seed = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        // デフォルト出力先は .gitignore で除外されている tmp-paifu/ 配下にする
        var outputDirectory = "./paifu";
        var writePaifu = true;
        // ベンチマーク結果 (32 対局 × 各並列度 1/2/4/8/12/16/24) で wall-clock が最小だった 4 を既定値に採用。
        // 論理コア 24 でも並列 8 以上では ThreadPool 競合でスループットが頭打ち/悪化する
        var parallelism = 4;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--games" when i + 1 < args.Length:
                    gameCount = int.Parse(args[++i]);
                    break;

                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i]);
                    break;

                case "--output" when i + 1 < args.Length:
                    outputDirectory = args[++i];
                    break;

                case "--no-paifu":
                    writePaifu = false;
                    break;

                case "--parallel" when i + 1 < args.Length:
                    parallelism = int.Parse(args[++i]);
                    break;
            }
        }

        return new AutoPlayOptions(gameCount, seed, outputDirectory, writePaifu, parallelism);
    }
}
