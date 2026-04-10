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
- **Mahjong.Lib** — 麻雀ドメインロジック。他プロジェクトへの依存なし
- **Mahjong.Lib.Tests** — Mahjong.Libのテスト（InternalsVisibleToで内部メンバーにアクセス可能）

### ソリューション構成（Mahjong4.slnx）

ソリューションフォルダで整理されている:
- **/Aspire/** — AppHost, ServiceDefaults
- **/Lib/** — Mahjong.Lib
- **/Tests/** — Mahjong.Lib.Tests
- ルート — ApiService, Web

### ドメインモデル

全ドメイン型はイミュータブル（record + ImmutableList）で設計されている。コレクション型（TileKindList, TileKindListList, CallList, Hand, FuList, YakuList）は`[CollectionBuilder]`属性によりコレクション式`[.. ]`での生成に対応し、ネストされたBuilderクラスを持つ。

TileKind、Fu、Yakuはstaticプロパティによるシングルトンインスタンスを持ち、コンストラクタはinternal。新しいインスタンスを`new`で作成せず、必ずシングルトンプロパティ（`TileKind.Man1`、`Fu.Futei`、`Yaku.Pinfu`等）経由で参照する。

#### 牌（Mahjong.Lib/Tiles/）

- **TileKind** — 牌種別（0-33の値）。萬子(0-8)・筒子(9-17)・索子(18-26)・風牌(27-30)・三元牌(31-33)。34個のstaticシングルトン（`TileKind.Man1`等）と分類用staticコレクション（All, Numbers, Mans, Pins, Sous, Honors, Winds, Dragons, Chunchans, Yaochus, Routous）を持つ
- **TileKindList** — 牌の集合。自動ソート済み。文字列コンストラクタ対応（`new TileKindList(man: "123", pin: "456")`）。面子判定プロパティ（IsToitsu, IsShuntsu, IsKoutsu, IsKantsu）と牌種判定（IsAllMan, IsAllSameSuit, IsAllYaochu, IsAllRoutou等）を持つ
- **TileKindListList** — TileKindListの集合。面子グループの管理に使用
- **Hand** — TileKindListListを継承。晒していない手牌を表現。副露との結合（CombineFuuro）や和了グループ取得（GetWinGroups）を提供

#### 副露（Mahjong.Lib/Calls/）

- **CallType** — 副露種別のenum（Chi, Pon, Ankan, Minkan, Nuki）。ToStr()拡張メソッドで日本語表記を返す
- **Call** — 副露を表現するrecord。コンストラクタで種類と牌構成の整合性を検証する。ファクトリメソッド（`Call.Chi(man: "123")`等）で簡潔に生成可能。IsOpenプロパティで門前判定
- **CallList** — 副露の集合。HasOpenで門前判定、TileKindListsで牌リストへの変換を提供

#### 符（Mahjong.Lib/Fus/）

- **FuType** — 符の種別enum。基本符（Futei=20, FuteiOpenPinfu=30, Chiitoitsu=25, Menzen=10）、待ち符（Tsumo, Kanchan, Penchan, Tanki=各2）、雀頭符（JantouPlayerWind, JantouRoundWind, JantouDragon=各2）、面子符（Minko/Anko/Minkan/Ankan × Chunchan/Yaochu=2〜32）
- **Fu** — 符を表現するrecord。FuTypeごとのstaticシングルトンインスタンス（`Fu.Futei`, `Fu.Tsumo`等）を持つ。Valueプロパティで符数を返す
- **FuList** — 符の集合。Totalプロパティで合計符数を計算（七対子は常に25符、それ以外は10の位に切り上げ）

#### 役（Mahjong.Lib/Yakus/）

- **Yaku** — 全役の抽象基底record。Number（天鳳準拠の役番号）、Name、HanOpen、HanClosed、IsYakumanの抽象プロパティを定義。60以上のシングルトンプロパティ（`Yaku.Riichi`、`Yaku.Pinfu`等）を持つ
- **YakuList** — 役の集合。HanOpen/HanClosedプロパティで合計翻数を計算
- **Yakus/Impl/** — 各役の具象実装（62ファイル）。各役はrecordでYakuを継承し、`static Valid()`メソッドで成立判定を行う。Validメソッドの引数は役ごとに異なる（Hand、CallList、FuList、WinSituation、GameRules等の必要な組み合わせ）

#### ゲーム設定（Mahjong.Lib/Games/）

- **KazoeLimit** — 数え役満の扱い（Limited/Sanbaiman/NoLimit）
- **Wind** — 風（東南西北）。TileKindへの変換拡張メソッドあり
- **GameRules** — ゲームルール設定（食いタン、ダブル役満、数え役満、切り上げ満貫、ピンヅモ、人和役満、大車輪等）
- **WinSituation** — 和了状況（ツモ・リーチ・一発等のフラグ、自風・場風、IsDealer計算プロパティ）

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
- テストはfeatureドメインごとのディレクトリ（Tiles/, Calls/, Fus/, Games/, Yakus/）に整理する
