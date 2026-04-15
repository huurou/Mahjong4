# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ビルド・テスト

```bash
# ビルド
dotnet build

# 全テスト実行
dotnet test

# 単一テストクラス実行
dotnet test --filter "FullyQualifiedName~TileKind_ConstructorTests"

# 単一テストメソッド実行
dotnet test --filter "FullyQualifiedName~TileKind_ConstructorTests.有効値0から33_正常に作成される"

# Aspireアプリ起動
dotnet run --project src/Mahjong.AppHost/Mahjong.AppHost.csproj

# 点数計算サンプルアプリ実行（Mahjong.Lib.Scoringの動作確認用コンソール）
dotnet run --project samples/Mahjong.Lib.Scoring.SampleApp/Mahjong.Lib.Scoring.SampleApp.csproj

# 点数計算検証ツール実行（対話的にYYYYMMDDの日付を入力）
dotnet run --project tools/Mahjong.Lib.Scoring.TenhouPaifuValidation/Mahjong.Lib.Scoring.TenhouPaifuValidation.csproj

# コードカバレッジ計測（PowerShell）
pwsh scripts/TestCoverage.ps1
```

テストランナーはMicrosoft.Testing.Platform（global.jsonで設定済み）。

## アーキテクチャ

.NET 10.0 + .NET Aspire構成の麻雀アプリケーション。

### プロジェクト構成

- **Mahjong.AppHost** — Aspireオーケストレーター。ApiServiceとWebを起動・管理する
- **Mahjong.ApiService** — REST API（OpenAPI対応）。ServiceDefaultsを参照
- **Mahjong.Web** — Blazor Serverフロントエンド。ApiServiceをサービスディスカバリ経由で参照
- **Mahjong.ServiceDefaults** — OpenTelemetry、サービスディスカバリ、レジリエンス等の共有設定
- **Mahjong.Lib.Scoring** — 麻雀の点数計算ドメインロジック。外部NuGet依存なし（追加時は慎重に判断する）
- **Mahjong.Lib.Game** — 麻雀の対局ロジック。Mahjong.Lib.Scoringには依存せず、対局進行ドメインとして独立している。牌・手牌・山・河・副露・プレイヤー情報・局の状態遷移を管理する
- **Mahjong.Lib.Scoring.Tests** — Mahjong.Lib.Scoringのテスト（InternalsVisibleToで内部メンバーにアクセス可能）
- **Mahjong.Lib.Game.Tests** — Mahjong.Lib.Gameのテスト。状態遷移の統合テストと各状態の単体テストを含む
- **Mahjong.Lib.Scoring.SampleApp** — `samples/`配下のコンソールアプリ。`HandCalculator.Calc`の代表的な入力例（役牌、立直平和ツモ一発、役満、副露等）を実行して結果を表示する動作確認用サンプル
- **Mahjong.Lib.Scoring.TenhouPaifuValidation**（`tools/`配下）— 天鳳牌譜をダウンロード・解析し、`HandCalculator`の点数計算結果を実データと突き合わせる検証コンソールアプリ
- **Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests** — 上記ツールのテスト

### ソリューション構成（Mahjong4.slnx）

ソリューションフォルダで整理されている:
- **/Aspire/** — AppHost, ServiceDefaults
- **/Lib/** — Mahjong.Lib.Scoring, Mahjong.Lib.Game
- **/Samples/** — Mahjong.Lib.Scoring.SampleApp
- **/Tools/** — Mahjong.Lib.Scoring.TenhouPaifuValidation
- **/Tests/** — Mahjong.Lib.Scoring.Tests, Mahjong.Lib.Game.Tests, Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests
- ルート — ApiService, Web

### 点数計算ドメインモデル（Mahjong.Lib.Scoring）

全ドメイン型はイミュータブル（record + ImmutableList）で設計されている。コレクション型（TileKindList, TileKindListList, CallList, Hand, FuList, YakuList）は`[CollectionBuilder]`属性によりコレクション式`[.. ]`での生成に対応し、ネストされたBuilderクラスを持つ。

TileKind、Fu、Yakuはstaticプロパティによるシングルトンインスタンスを持ち、コンストラクタはinternal。新しいインスタンスを`new`で作成せず、必ずシングルトンプロパティ（`TileKind.Man1`、`Fu.Futei`、`Yaku.Pinfu`等）経由で参照する。

**牌種別(TileKind)と牌(Tile)の区別**: TileKindは牌の絵柄（種別）を表し、点数計算にはこれのみで十分。Tileは同じ絵柄でも1枚1枚を区別する（実際のゲームや赤ドラで必要）。詳細は`docs/Design.md`参照。

#### 牌（Mahjong.Lib.Scoring/Tiles/）

- **TileKind** — 牌種別（0-33の値、34個のstaticシングルトン）。文字列コンストラクタ（`new TileKindList(man: "123", pin: "456")`）で手牌を簡潔に生成可能
- **TileKindList** — 牌の集合。自動ソート済み。面子判定・牌種判定プロパティを持つ
- **TileKindListList** — TileKindListの集合。面子グループの管理に使用
- **Hand** — TileKindListListを継承。晒していない手牌を表現

#### 副露（Mahjong.Lib.Scoring/Calls/）

- **Call** — 副露を表現するrecord。ファクトリメソッド（`Call.Chi(man: "123")`等）で生成。コンストラクタで種類と牌構成の整合性を検証
- **CallList** — 副露の集合。HasOpenで門前判定

#### 符（Mahjong.Lib.Scoring/Fus/）

- **Fu** — 符を表現するrecord。FuTypeごとのstaticシングルトン（`Fu.Futei`, `Fu.Tsumo`等）
- **FuList** — 符の集合。Totalで合計符数を計算（七対子は常に25符、それ以外は10の位に切り上げ）

#### 役（Mahjong.Lib.Scoring/Yakus/）

- **Yaku** — 全役の抽象基底record。シングルトンプロパティ（`Yaku.Riichi`、`Yaku.Pinfu`等）で参照
- **YakuList** — 役の集合。HanOpen/HanClosedで合計翻数を計算
- **Yakus/Impl/** — 各役の具象実装。`static Valid()`メソッドで成立判定。引数は役ごとに異なる（Hand、CallList、WinSituation、GameRules等の必要な組み合わせ）

#### 向聴数（Mahjong.Lib.Scoring/Shantens/）

- **ShantenCalculator** — 公開入口。`Calc(TileKindList, useRegular, useChiitoitsu, useKokushi)`で通常形・七対子・国士無双を切り替えて最小向聴数を返す。`SHANTEN_TENPAI=0`、`SHANTEN_AGARI=-1`
- 内部ではShantenContext（再帰的な面子・塔子探索）とIsolationSet（孤立牌管理）を使用

#### 和了計算（Mahjong.Lib.Scoring/HandCalculating/）

- **HandCalculator** — 公開入口（`static`）。`Calc(tileKindList, winTile, callList?, doraIndicators?, uradoraIndicators?, winSituation?, gameRules?)`で役・符・点数を含む`HandResult`を返す
- 計算パイプラインは段階的に進む: `HandValidator`（入力検証・和了形チェック） → `SpecialHandEvaluator`（流し満貫・国士無双などの特殊役） → `HandDivider`で手牌を面子分解 → 各分解候補ごとに`YakuEvaluator`（役判定）・`FuCalculator`（符計算）・`ScoreCalculator`（点数計算） → **高点法**で最高点の分解候補を選択
- **HandResult** — 結果`record`（Fu, Han, Score, YakuList, FuList, ErrorMessage）。`HandResult.Create(...)`は役が0翻のとき自動で`Error("役がありません。")`を返す
- **Score** — `record Score(int Main, int Sub = 0)`。Main/Subは和了方（親/子、ツモ/ロン）で意味が変わる（XMLコメント参照）
- **HandDividing/HandDivider** — 手牌を可能な面子・雀頭の組み合わせに分解。複数解を返すため高点法が必要
- **FuCalculator**（`Mahjong.Lib.Scoring/Fus/`配下）— 雀頭・面子・待ち・ベース符を組み立ててFuListを生成。七対子は常に25符固定

#### ゲーム設定（Mahjong.Lib.Scoring/Games/）

- **GameRules** — ゲームルール設定（食いタン、ダブル役満、数え役満、切り上げ満貫等）
- **WinSituation** — 和了状況（ツモ・リーチ・一発等のフラグ、自風・場風）
- **Wind** — 風（東南西北）。TileKindへの変換拡張メソッドあり

### 対局ドメインモデル（Mahjong.Lib.Game）

Mahjong.Lib.Scoringが点数計算に特化するのに対し、Mahjong.Lib.Gameは対局進行を担当する。両者は独立しており、Mahjong.Lib.GameはMahjong.Lib.Scoringを参照しない（対局進行時に`TileKind`が必要になれば、牌ID (0-135) の `Id / 4` でLib.Scoring側の牌種インデックスへ変換可能）。

**牌種別(TileKind)と牌(Tile)の関係**: `Tile`は0-135のIDで136枚の牌を個別に識別する。点数計算には34種の絵柄インデックスのみで十分だが、対局進行では同種牌の区別（赤ドラ等）が必要なため`Tile`を使用する。

#### 牌・手牌・河（Mahjong.Lib.Game/Tiles/, Hands/, Rivers/）

- **Tile** — 牌1枚を表すrecord（ID: 0-135）。`Id / 4`で`TileKind`を導出
- **Hand** — 未公開の手牌を表現するrecord。`ImmutableList<Tile>`をラップし`IEnumerable<Tile>`を実装（`AddTile`/`RemoveTile`でイミュータブル更新）
- **River** — 河（捨て牌）を表現するrecord。`ImmutableArray<Tile>`をラップし`IEnumerable<Tile>`を実装（`Add`/`RemoveLast`）

#### 副露（Mahjong.Lib.Game/Calls/）

- **CallType** — 副露の種類を列挙（Chi, Pon, Ankan, Daiminkan, Kakan）。Lib.Scoringの副露と異なり大明槓と加槓を区別する（表示上の違いがあるため）
- **Call** — `record Call(CallType Type, ImmutableList<Tile> Tiles, PlayerIndex From, Tile CalledTile)`
- **CallList** — Callのイミュータブルコレクション

#### 山（Mahjong.Lib.Game/Walls/）

- **Wall** — 山牌を表すrecord。`Draw`/`DrawRinshan`/`RevealDora`でイミュータブルに操作
- **IWallGenerator** — 山生成のインターフェース。`Generate()`で`Wall`を返す
- **WallGeneratorTenhou** — 天鳳互換の山生成。Mersenne Twister (MT19937) + SHA512ハッシュ圧縮で牌をシャッフル

#### プレイヤー（Mahjong.Lib.Game/Players/）

- **PlayerIndex** — `record PlayerIndex(int Value)`。プレイヤーの席順（0-3）。`Next()`で次家へ
- **Point** — `record Point(int Value)`。持ち点

#### プレイヤー別配列（各ドメイン配下の `*Array` 型）

4プレイヤー分の状態を`ImmutableArray`で保持するイミュータブルな配列ラッパー。`PlayerIndex`でインデックス参照し、更新メソッドは新しいインスタンスを返す。

- **HandArray** (`Hands/`) — 各プレイヤーの`Hand`配列（`AddTile`/`AddTiles`/`RemoveTile`）
- **RiverArray** (`Rivers/`) — 各プレイヤーの`River`配列（`AddTile`/`RemoveLastTile`）
- **CallListArray** (`Calls/`) — 各プレイヤーの`CallList`配列（`AddCall`/`ReplaceCall`）
- **PointArray** (`Players/`) — 各プレイヤーの持ち点配列

#### 局（Mahjong.Lib.Game/Rounds/）

局全体の状態を1つのrecordに集約し、進行アクションはすべて新しい`Round`を返すイミュータブル設計。`RoundState`（状態機械）が`Round`を受け取り各アクションを呼び出す構造。

- **Round** — 局の状態を保持するrecord（`RoundWind`/`RoundNumber`/`Honba`/`KyoutakuRiichiCount`/`Turn`/`PointArray`/`IWallGenerator`/`Wall`/`HandArray`/`CallListArray`/`RiverArray`/`PendingDoraReveal`）。`Haipai`/`Tsumo`/`Dahai`/`NextTurn`/`Chi`/`Pon`/`Daiminkan`/`Ankan`/`Kakan`/`RinshanTsumo`/`RevealDora`/`SetPoints`/`AddKyoutakuRiichi`/`ClearKyoutaku`の各メソッドで進行
- **RoundWind** — 場風
- **RoundNumber** — 局数。`ToDealer()`で親の`PlayerIndex`を返す
- **Honba** — 本場
- **KyoutakuRiichiCount** — 供託リーチ棒の本数

**カンドラ表示のタイミング**: 暗槓は即時`RevealDora`、加槓・大明槓は`PendingDoraReveal=true`で保留し、次の`RinshanTsumo`直前にめくる

#### 局の状態遷移（Mahjong.Lib.Game/States/RoundStates/）

非同期イベント駆動のステートマシンで局の進行を管理する。`System.Threading.Channels.Channel<T>`によるイベントキューでスレッドセーフな状態遷移を実現。

- **RoundState** — 状態の抽象基底クラス。`ResponseOk`/`ResponseWin`/`ResponseDahai`/`ResponseKan`/`ResponseCall`/`ResponseRyuukyoku`の仮想メソッドを持つ。`Entry`/`Exit`ライフサイクルメソッドと`Transit`ヘルパーで遷移を制御
- **RoundEvent** — イベントの抽象基底クラス。6種の具象実装（ResponseOk, ResponseDahai, ResponseCall, ResponseWin, ResponseKan, ResponseRyuukyoku）
- **RoundStateContext** — ステートマシン本体。`Init()`で`RoundStateHaipai`から開始。イベントをChannelに投入し、バックグラウンドタスクで順次処理。`RoundStateChanged`イベントで状態遷移を通知。`IDisposable`で5秒タイムアウト付きの安全なシャットダウン

**状態遷移フロー**（詳細は`docs/Design.md`の状態遷移図を参照）:
- 配牌(`Haipai`) → ツモ(`Tsumo`) → 打牌(`Dahai`) → ツモ（次のプレイヤー）のサイクルが基本
- 打牌に対して副露(`Call`)・ロン和了(`Win`)・流局(`Ryuukyoku`)が分岐
- ツモに対して暗槓/加槓(`Kan`) → 槓ツモ(`KanTsumo`) → 槓ツモ後(`AfterKanTsumo`)の槓サイクル
- `Win`と`Ryuukyoku`が終端状態

### 点数計算検証ツール（tools/Mahjong.Lib.Scoring.TenhouPaifuValidation/）

`HandCalculator`の実装を天鳳の実牌譜に対して総当たり検証するためのコンソールアプリ。`Microsoft.Extensions.DependencyInjection`でサービスを組み立て、対話的に入力された日付（YYYYMMDD）の牌譜を処理する。

パイプラインは`UseCase`を起点に2段階:
1. **AnalysisPaifu** — `PaifuDownloadService`が天鳳から牌譜をダウンロード → `RoundDataExtractService`が局単位に分解 → `InitParseService`/`AgariParseService`/`MeldParseService`がXMLタグをドメインモデルに変換 → `AgariInfoBuildService`が`AgariInfo`を生成
2. **ValidateCalc** — `CalcValidateService`が`AgariInfo`から`HandCalculator.Calc`を呼び出し、実データの符・翻・点数と突き合わせて`ValidateResult`を返す

テストデータは`tests/Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests/TestData/`配下のXML牌譜（途中流局有無・ダブロン等のバリエーション）。

## 作業上の注意

- `.claude/settings.json`のStopフックにより、セッション終了時にソースファイルがUTF-8 BOM付きに自動変換される

## コーディング規約

- プライベートフィールド: キャメルケース + `_`サフィックス（例: `myField_`）
- 定数（const）: UPPER_SNAKE_CASE
- LINQの引数にはx, y, zを使用

## テスト規約

- フレームワーク: xUnit v3（`xunit.v3.mtp-v2`）
- テストクラス命名: `対象クラス_対象メソッドTests`（例: `TileKind_ConstructorTests`）
- テストメソッド命名: 日本語で「条件_期待される挙動」（例: `有効値0から33_正常に作成される`）
- AAA（Arrange-Act-Assert）パターンを使用
- 例外テストは`Record.Exception`を使用（`Assert.Throws`は使わない）
- Fluent Assertionは使用禁止
- テストコードにドキュメントコメントを付与しない
- テストはfeatureドメインごとのディレクトリ（Tiles/, Calls/, Fus/, Games/, Yakus/, Shantens/, HandCalculating/）に整理する
- `HandCalculator.Calc`のテストは入力カテゴリ別にクラスを分割（例: `HandCalculator_CalcTests_Shuntsu`、`_Koutsu`、`_Kokushimusou`、`_Dora`、`_Error`、`_Formless`、`_Tenhou`、`_Others`）
