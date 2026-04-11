namespace Mahjong.Lib.HandCalculating.Scores;

/// <summary>
/// 点数 <para/>
/// 親の満貫ツモアガリ: { 4000, 0 } <br/>
/// 子の満貫ツモアガリ: { 4000, 2000 } <br/>
/// 親の満貫ロンアガリ: { 12000, 0 } <br/>
/// 子の満貫ロンアガリ: { 8000, 0 }
/// </summary>
/// <param name="Main">親のツモ: 子の点数 子のツモ: 親の点数 ロン:手の点数</param>
/// <param name="Sub">親のツモ: 0 子のツモ: 子の点数 ロン: 0</param>
public record Score(int Main, int Sub = 0);
