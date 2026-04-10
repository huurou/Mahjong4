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

### ドメインモデル（Mahjong.Lib/Tiles/）

イミュータブル（record + ImmutableList）で設計されている。

- **TileKind** — 牌種別（0-33の値）。萬子(0-8)・筒子(9-17)・索子(18-26)・風牌(27-30)・三元牌(31-33)
- **TileKindList** — 牌の集合。文字列コンストラクタ対応（`new TileKindList(man: "123", pin: "456")`）。面子判定プロパティ（IsToitsu, IsShuntsu, IsKoutsu, IsKantsu）を持つ
- **TileKindListList** — TileKindListの集合。面子グループの管理に使用

### ドメインモデル（Mahjong.Lib/Games/）

- **KazoeLimit** — 数え役満の扱い（Limited/Sanbaiman/NoLimit）
- **Wind** — 風（東南西北）。TileKindへの変換拡張メソッドあり
- **GameRules** — ゲームルール設定（食いタン、ダブル役満、数え役満等）
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
