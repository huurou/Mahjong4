using System.Security.Cryptography;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// 自動対局オプション
/// </summary>
/// <param name="GameCount">対局数</param>
/// <param name="Seed">乱数シード (IWallGenerator / AI 共通)</param>
/// <param name="OutputDirectory">牌譜出力先ディレクトリ</param>
/// <param name="WritePaifu">牌譜を書き出すか</param>
public record AutoPlayOptions(int GameCount, int Seed, string OutputDirectory, bool WritePaifu)
{
    public static AutoPlayOptions Parse(string[] args)
    {
        var gameCount = 20;
        var seed = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        // デフォルト出力先は .gitignore で除外されている tmp-paifu/ 配下にする
        var outputDirectory = "./tmp-paifu";
        var writePaifu = false;

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

                case "--write-paifu":
                    writePaifu = true;
                    break;
            }
        }

        return new AutoPlayOptions(gameCount, seed, outputDirectory, writePaifu);
    }
}
