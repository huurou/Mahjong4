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

# 点数計算サンプルアプリ実行（Mahjong.Libの動作確認用コンソール）
dotnet run --project samples/Mahjong.Lib.ScoringSampleApp/Mahjong.Lib.ScoringSampleApp.csproj

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
- **Mahjong.Lib** — 麻雀ドメインロジック。外部NuGet依存なし（追加時は慎重に判断する）
- **Mahjong.Lib.Tests** — Mahjong.Libのテスト（InternalsVisibleToで内部メンバーにアクセス可能）
- **Mahjong.Lib.ScoringSampleApp** — `samples/`配下のコンソールアプリ。`HandCalculator.Calc`の代表的な入力例（役牌、立直平和ツモ一発、役満、副露等）を実行して結果を表示する動作確認用サンプル

### ソリューション構成（Mahjong4.slnx）

ソリューションフォルダで整理されている:
- **/Aspire/** — AppHost, ServiceDefaults
- **/Lib/** — Mahjong.Lib
- **/Samples/** — Mahjong.Lib.ScoringSampleApp
- **/Tests/** — Mahjong.Lib.Tests
- ルート — ApiService, Web

### ドメインモデル

全ドメイン型はイミュータブル（record + ImmutableList）で設計されている。コレクション型（TileKindList, TileKindListList, CallList, Hand, FuList, YakuList）は`[CollectionBuilder]`属性によりコレクション式`[.. ]`での生成に対応し、ネストされたBuilderクラスを持つ。

TileKind、Fu、Yakuはstaticプロパティによるシングルトンインスタンスを持ち、コンストラクタはinternal。新しいインスタンスを`new`で作成せず、必ずシングルトンプロパティ（`TileKind.Man1`、`Fu.Futei`、`Yaku.Pinfu`等）経由で参照する。

**牌種別(TileKind)と牌(Tile)の区別**: TileKindは牌の絵柄（種別）を表し、点数計算にはこれのみで十分。Tileは同じ絵柄でも1枚1枚を区別する（実際のゲームや赤ドラで必要）。詳細は`docs/Design.md`参照。

#### 牌（Mahjong.Lib/Tiles/）

- **TileKind** — 牌種別（0-33の値、34個のstaticシングルトン）。文字列コンストラクタ（`new TileKindList(man: "123", pin: "456")`）で手牌を簡潔に生成可能
- **TileKindList** — 牌の集合。自動ソート済み。面子判定・牌種判定プロパティを持つ
- **TileKindListList** — TileKindListの集合。面子グループの管理に使用
- **Hand** — TileKindListListを継承。晒していない手牌を表現

#### 副露（Mahjong.Lib/Calls/）

- **Call** — 副露を表現するrecord。ファクトリメソッド（`Call.Chi(man: "123")`等）で生成。コンストラクタで種類と牌構成の整合性を検証
- **CallList** — 副露の集合。HasOpenで門前判定

#### 符（Mahjong.Lib/Fus/）

- **Fu** — 符を表現するrecord。FuTypeごとのstaticシングルトン（`Fu.Futei`, `Fu.Tsumo`等）
- **FuList** — 符の集合。Totalで合計符数を計算（七対子は常に25符、それ以外は10の位に切り上げ）

#### 役（Mahjong.Lib/Yakus/）

- **Yaku** — 全役の抽象基底record。シングルトンプロパティ（`Yaku.Riichi`、`Yaku.Pinfu`等）で参照
- **YakuList** — 役の集合。HanOpen/HanClosedで合計翻数を計算
- **Yakus/Impl/** — 各役の具象実装。`static Valid()`メソッドで成立判定。引数は役ごとに異なる（Hand、CallList、WinSituation、GameRules等の必要な組み合わせ）

#### 向聴数（Mahjong.Lib/Shantens/）

- **ShantenCalculator** — 公開入口。`Calc(TileKindList, useRegular, useChiitoitsu, useKokushi)`で通常形・七対子・国士無双を切り替えて最小向聴数を返す。`SHANTEN_TENPAI=0`、`SHANTEN_AGARI=-1`
- 内部ではShantenContext（再帰的な面子・塔子探索）とIsolationSet（孤立牌管理）を使用

#### 和了計算（Mahjong.Lib/HandCalculating/）

- **HandCalculator** — 公開入口（`static`）。`Calc(tileKindList, winTile, callList?, doraIndicators?, uradoraIndicators?, winSituation?, gameRules?)`で役・符・点数を含む`HandResult`を返す
- 計算パイプラインは段階的に進む: `HandValidator`（入力検証・和了形チェック） → `SpecialHandEvaluator`（流し満貫・国士無双などの特殊役） → `HandDivider`で手牌を面子分解 → 各分解候補ごとに`YakuEvaluator`（役判定）・`FuCalculator`（符計算）・`ScoreCalculator`（点数計算） → **高点法**で最高点の分解候補を選択
- **HandResult** — 結果`record`（Fu, Han, Score, YakuList, FuList, ErrorMessage）。`HandResult.Create(...)`は役が0翻のとき自動で`Error("役がありません。")`を返す
- **Score** — `record Score(int Main, int Sub = 0)`。Main/Subは和了方（親/子、ツモ/ロン）で意味が変わる（XMLコメント参照）
- **HandDividing/HandDivider** — 手牌を可能な面子・雀頭の組み合わせに分解。複数解を返すため高点法が必要
- **FuCalculator**（`Mahjong.Lib/Fus/`配下）— 雀頭・面子・待ち・ベース符を組み立ててFuListを生成。七対子は常に25符固定

#### ゲーム設定（Mahjong.Lib/Games/）

- **GameRules** — ゲームルール設定（食いタン、ダブル役満、数え役満、切り上げ満貫等）
- **WinSituation** — 和了状況（ツモ・リーチ・一発等のフラグ、自風・場風）
- **Wind** — 風（東南西北）。TileKindへの変換拡張メソッドあり

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
